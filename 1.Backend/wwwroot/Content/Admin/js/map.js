var defaultLatLng = { lat: 21.0277644, lng: 105.83415979999995 };
var SInterval = 3 * 1000;
var closeMenuLeft = false;
var closeMenuRight = true;
var map;
var markers = [];
var markersMini = [];
var stations = [];
var curentStationId = 0;
var iconStationOpen;
var iconStationClose;
var currentPopup;

function initMap() {
    var firsLatLng = new google.maps.LatLng(defaultLatLng.lat, defaultLatLng.lng);
    var nopoi = [
        {
            featureType: "poi",
            stylers: [
                { visibility: "off" }
            ]
        },
        {
            featureType: 'transit.station',
            stylers: [
                { visibility: "off" }
            ]
        },
    ];
    map = new google.maps.Map(document.getElementById('map'), {
        center: firsLatLng,
        zoom: 15,
        mapTypeControl: false,
        styles: nopoi
    });

    iconStationOpen = {
        url: "/Content/Admin/images/icon-station-on.png",
        scaledSize: new google.maps.Size(30, 30),
        origin: new google.maps.Point(0, 0), // origin
        anchor: new google.maps.Point(20, 20), // anchor,
        //labelOrigin: new google.maps.Point(12, -10)
    };
    iconStationClose = {
        url: "/Content/Admin/images/icon-station-off.png",
        scaledSize: new google.maps.Size(30, 30),
        origin: new google.maps.Point(0, 0), // origin
        anchor: new google.maps.Point(20, 20), // anchor,
      //  labelOrigin: new google.maps.Point(12, -10)
    };
    getDataStation(function (data) {
        if (data.status == true) {
            bindMenuLeft(data.data);
            addMarker(stations);
        }
    });
}
function getDataStation(callBack) {
    $.get("/Home/Stations", function (data) {
        stations = data.data;
        callBack(data);
    });
}

setInterval(function () {
    updateData();
}, SInterval);
function formatMarkerPopup(data) {
    return `<h3 class='tenTram'>` + data.name +`</h3>`;
}
function updateData() {
    getDataStation(function (data) {
        if (data.status == true) {
            stations = data.data;
            changeSearch();
            for (var i = 0; i < markers.length; i++) {
                var ktStation = data.data.find(m => m.id == markers[i].id);
                if (ktStation != null && ktStation != undefined) {
                    markers[i].setIcon(ktStation.connectionStatus == false ? iconStationClose : iconStationOpen);
                    markers[i].popup.setContent(formatMarkerPopup(ktStation));
                }
            }
            bindInfoStation(curentStationId);
            bindInfoDocks(curentStationId);

        }
    });
}

function bindMenuLeft(data) {
    var strStation = "";
    $(data).each(function (i, item) {
        strStation += `
                    <li data-id='`+ item.id + `' class="list-group-item ` + (item.id == curentStationId?"active":"") + `" onclick="moveToStation(this,` + item.id + `,` + item.lng + `,` + item.lat + `)">
                        `+ (item.connectionStatus == false ? '<i title="Mất kết nối" class="material-icons md-24 col-red">sync_disabled</i>' : '<i title="Có kết nối" class="material-icons md-24 col-green">swap_horiz</i>') + `
                        `+ item.name + `
                        <span class="badge badge-map-t bg-pink">`+ item.errorDock + `/` + item.totalDock + `</span>
                    </li>`;
    });
    $("#bindStation").html(strStation);
}
function initScrollRightMenu() {
    var viewportHeight = $("#contentMap").height() - 34 - 20;
    $('#accordion_1').slimScroll({
        height: viewportHeight + 'px',
        color: '#455A64',
        distance: '0',
        allowPageScroll: true,
        alwaysVisible: true
    });
}
function initScrollLeftMenu() {
    var viewportHeight = $("#contentMap").height() - 34 - 20 - 50 -35;
    $('.slimStation').slimScroll({
        height: viewportHeight + 'px',
        color: '#455A64',
        distance: '0',
        allowPageScroll: true,
        alwaysVisible: true
    });
}
$(".miniMenu.clickleftmenu").click(function () {
    if (closeMenuLeft) {
        $(".leftmenu").removeClass("close");
    } else {
        $(".leftmenu").addClass("close");
    }
    closeMenuLeft = !closeMenuLeft;
});
$(".miniMenu.clickrightmenu").click(function () {
    if (closeMenuRight) {
        $(".rightContent").removeClass("close");
    } else {
        $(".rightContent").addClass("close");
    }
    closeMenuRight = !closeMenuRight;
});
function addMarker(location) {
    for (var k in location) {
        var marker = new google.maps.Marker({
            position: {
                lat: parseFloat(location[k].lat),
                lng: parseFloat(location[k].lng)
            },
            map: map,
            title: location[k].name,
            icon: location[k].connectionStatus == false ? iconStationClose : iconStationOpen
            //labelClass: "labelGoogle",
            //label: {
            //    text: location[k].name,
            //    color: "#333",
            //    fontSize: "11px",
            //    fontWeight: "500",
            //    textShadow: "-1px 0 black, 0 1px black, 1px 0 black, 0 -1px black"
            //}
        });
        attachSecretMessage(marker, location[k]);
        marker.id = location[k].id;
        markers.push(marker);
    }
}
function attachSecretMessage(marker, secretMessage) {
    var infowindow = new google.maps.InfoWindow({
        content: formatMarkerPopup(secretMessage)
    });
    marker.popup = infowindow;
    marker.addListener('click', function () {
        for (var i = 0; i < markers.length; i++) {
            markers[i].popup.close();
        }

        currentPopup = infowindow;

        $(".slimStation li").removeClass("active");
        $("#bindStation .list-group-item[data-id='" + secretMessage.id+"']").addClass("active");
        curentStationId = secretMessage.id;
        $(".rightContent").removeClass("close");
        bindInfoStation(secretMessage.id);
        bindInfoDocks(secretMessage.id);

        infowindow.open(marker.get('map'), marker);
    });
}
function moveToStation($this, id, lng, lat) {
    if (currentPopup)
        currentPopup.close();
    map.setCenter({ lat: parseFloat(lat), lng: parseFloat(lng) });
    for (var i = 0; i < markers.length; i++) {
        var marker = markers[i];
        if (marker.id == id) {
            marker.popup.open(marker.get('map'), marker);
            currentPopup = marker.popup;
            break;
        }
    }
    $(".slimStation li").removeClass("active");
    $($this).addClass("active");
    curentStationId = id;
    $(".rightContent").removeClass("close");
    bindInfoStation(id);
    bindInfoDocks(id);
}
function bindInfoStation(id) {
    var strInfo = "";
    var curentStation = stations.find(x => x.id == id);
    if (curentStation != null && stations != undefined) {
        strInfo = `
                <div>
                    <div class="labelinfo-t bg-info mb-10">Thông tin cơ bản</div>
                    <div class='rowinfo'><span class='titleInfo'>Tên trạm: </span><span class='valueInfo'>`+ curentStation.name + `</span></div>
                    <div class='rowinfo'><span class='titleInfo'>Địa chỉ: </span><span class='valueInfo'>`+ curentStation.address + `</span></div>
                    <div class='rowinfo'><span class='titleInfo'>IMEI: </span><span class='valueInfo'>`+ curentStation.imei + `</span></div>
                    <div class='rowinfo'><span class='titleInfo'>Model: </span><span class='valueInfo'>`+ curentStation.model + `</span></div>
                    <div class='rowinfo'><span class='titleInfo'>Serial Number: </span><span class='valueInfo'>`+ curentStation.serialNumber + `</span></div>
                    <div class="labelinfo-t bg-info mb-10">Thông tin theo dõi</div>
                    <div class='rowinfo'><span class='titleInfo'>Dock lỗi: </span><span><b>`+ curentStation.errorDock + "/" + curentStation.totalDock + `</b></span></div>
                    <div class='rowinfo'><span class='titleInfo'>Trạng thái kết nối: </span>`+ (curentStation.connectionStatus == true ? "<span class='label label-success'>Đang kết nối</span>" : "<span class='label label-danger'>Mất kết nối</span>") + `</div>
                <div>
                `;
    }

    $("#tab2").html(strInfo);
}
function bindInfoDocks(id) {

    var strInfo = "";
    var curentStation = stations.find(x => x.id == id);
    if (curentStation != null && stations != undefined) {

        $(curentStation.docks).each(function (i, item) {
            var imei = "---";
            if (item.currentBike != null) {
                imei = item.currentBike.imei;
            }
            strInfo +=
                `
            <div class="panel panel-default panel-dock">
                <div class="panel-heading" role="tab" id="heading_2">
                    <h4 class="panel-title">
                        <a style='padding-left: 4px;' class="collapsed" role="button" data-toggle="collapse" data-parent="#accordion_1" href="#collapse_`+ item.id + `" aria-expanded="false" aria-controls="collapse_`+ item.id + `">
                           <span class='namedock `+ (item.status == 1 ? "on" : "off") + `'>` + item.orderNumber + `</span>
                           <i style='margin-right: 2px;' class="material-icons `+ (item.lockStatus == true ? "col-green" : "col-red") +` icon-doc">`+ (item.lockStatus == true ? "lock_open" :"lock_outline") +`</i>
                           <span class='styledockname'>` + item.serialNumber + `</span>
                           <img  class='styleimgbikein `+ (item.bikeAlarm == true ? "" : "hidden") + `' src="/Content/Admin/images/bikein.png" />
                        </a>
                    </h4>
                </div>
                <div id="collapse_`+ item.id + `" class="panel-collapse collapse" role="tabpanel" aria-labelledby="heading_2">
                    <div class="panel-body" style='padding: 10px;'>
                        <div class='rowinfo'><span class='titleInfo'>IMEI: </span><span class='valueInfo'>`+ item.imei + `</span></div>
                        <div class='rowinfo'><span class='titleInfo'>Model: </span><span class='valueInfo'>`+ item.model + `</span></div>
                        <div class='rowinfo'><span class='titleInfo'>Số serial: </span><span class='valueInfo'>`+ item.serialNumber + `</span></div>
                        <div class='rowinfo'><span class='titleInfo'>Thứ tự: </span><span class='valueInfo'>`+ item.orderNumber + `</span></div>
                        <div class='rowinfo'><span class='titleInfo'>Tình trạng: </span>`+ (item.bikeAlarm == true ? "<span class='label label-success'>Có xe trong dock</span>" : "<span class='label label-default'>Không có xe</span>") + `</div>
                        <div class='rowinfo'><span class='titleInfo'>Xe trên dock: </span><span  class='valueInfo'>`+ imei + `</span></div>
                        <div class='rowinfo'><span class='titleInfo'>Trạng thái: </span>`+ (item.status == 1 ? "<span class='label label-success'>Đang hoạt động</span>" : "<span class='label label-danger'>Bi hỏng</span>") + `</div>
                        <div class='rowinfo'><span class='titleInfo'>Trạng thái khóa: </span>`+ (curentStation.lockStatus == false ? "<span class='label label-info'>Đang khóa</span>" : "<span class='label label-default'>Đang mở</span>") + `</div>
                    </div>
                </div>
            </div>
        `;
        });
    }
    $("#tab1 .panel-group").html(strInfo);
}
function changeSearch() {
    var cnnStatus = $("#connectionStatus").val();
    var key = $("#key").val();
    var newData = stations;
    if (key != "") {
        newData = newData.filter(m => m.name.toLowerCase().includes(key.toLowerCase()) || (m.serialNumber.toLowerCase().includes(key.toLowerCase())) || (m.imei.toLowerCase().includes(key.toLowerCase())));
    }
    if (cnnStatus == "true") {
         newData = stations.filter(m => m.connectionStatus == true);
    }
    else if (cnnStatus == "false") {
         newData = stations.filter(m => m.connectionStatus == false);
    }
    else {
    }
    bindMenuLeft(newData);
}
initScrollLeftMenu();
initScrollRightMenu();