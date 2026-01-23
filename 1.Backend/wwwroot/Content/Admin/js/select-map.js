var map;
var marker;
var markers = [];

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
        }
    ];
    map = new google.maps.Map(document.getElementById('mapStation'), {
        center: firsLatLng,
        zoom: 15,
        mapTypeControl: false,
        styles: nopoi
    });
    google.maps.event.addListener(map, 'click', function (event) {
        mapZoom = map.getZoom();
        placeMarker(event.latLng, true, false);
    });
    initSearchMap();
}

function initSearchMap() {
    var input = document.getElementById('pac-input');
    var searchBox = new google.maps.places.SearchBox(input);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    // create a manual button to the right of the input for manual search
    var btn = document.createElement('button');
    btn.type = 'button';
    btn.id = 'pac-button';
    btn.title = 'Tìm và chọn vị trí';
    btn.innerHTML = '<i class="material-icons">search</i>';
    // basic styling so it appears on the map control area
    btn.style.marginLeft = '6px';
    btn.style.height = '34px';
    btn.style.padding = '4px 8px';
    btn.style.background = '#fff';
    btn.style.border = '1px solid #ccc';
    btn.style.cursor = 'pointer';
    btn.style.borderRadius = '2px';
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(btn);

    // when user changes map bounds, bias the SearchBox results
    map.addListener('bounds_changed', function () {
        searchBox.setBounds(map.getBounds());
    });

    // When the user selects a prediction from the picker
    searchBox.addListener('places_changed', function () {
        var places = searchBox.getPlaces();
        if (places.length === 0) {
            return;
        }
        var bounds = new google.maps.LatLngBounds();
        places.forEach(function (place) {
            if (!place.geometry) {
                console.log("Returned place contains no geometry");
                return;
            }
            if (place.geometry.viewport) {
                bounds.union(place.geometry.viewport);
            } else {
                bounds.extend(place.geometry.location);
            }
        });
        if (places.length > 0) {
            placeMarker(places[0].geometry.location, true);
        }
        map.fitBounds(bounds);
    });

    // helper: geocode an address string and place marker
    function geocodeAddress(address) {
        if (!address || address.trim() === '') {
            alertify && alertify.notify && alertify.notify('Vui lòng nhập địa chỉ', 'warning', 3);
            return;
        }
        var geocoder = new google.maps.Geocoder();
        geocoder.geocode({ address: address }, function (results, status) {
            if (status === google.maps.GeocoderStatus.OK && results[0]) {
                var loc = results[0].geometry.location;
                placeMarker(loc, true);
                map.setCenter(loc);
                // if viewport available, adjust
                if (results[0].geometry.viewport) {
                    map.fitBounds(results[0].geometry.viewport);
                }
            } else {
                console.log('Geocode was not successful for the following reason: ' + status);
                alertify && alertify.notify && alertify.notify('Không tìm thấy địa chỉ', 'warning', 3);
            }
        });
    }

    // pressing Enter in the input triggers geocode
    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.keyCode === 13) {
            e.preventDefault();
            geocodeAddress(input.value);
        }
    });

    // clicking the manual button triggers geocode
    btn.addEventListener('click', function (e) {
        e.preventDefault();
        geocodeAddress(input.value);
    });
}

function initMarker(lat, lng) {
    var firsLatLng;
    if (lat === 0 || lng === 0) {
        firsLatLng = new google.maps.LatLng(defaultLatLng.lat, defaultLatLng.lng);
    }
    else {
        firsLatLng = new google.maps.LatLng(lat, lng);
    }
    placeMarker(firsLatLng, false);
}

function initMarkerView(lat, lng) {
    var firsLatLng;
    if (lat === 0 || lng === 0) {
        firsLatLng = new google.maps.LatLng(defaultLatLng.lat, defaultLatLng.lng);
    }
    else {
        firsLatLng = new google.maps.LatLng(lat, lng);
    }
    placeMarker(firsLatLng, false, true, false);
}

function removeMarkers() {
    if (markers.length > 0) {
        for (i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
    }
}

function eventDragEnd(e) {
    document.getElementById('Lat').value = this.position.lat();
    document.getElementById('Lng').value = this.position.lng();
    alertify.notify("Di chuyển tọa độ thành công, có thể đóng cửa sổ này và click 'Lưu thay đổi'", "success", 5);
}

function placeMarker(location, isInfo = true, isSetCenter = true, isSetValue = true) {
    removeMarkers();
    marker = new google.maps.Marker({
        position: location,
        map: map,
        draggable: true,
        title: "Tọa độ"
    });
    markers.push(marker);
    if (isSetValue) {
        marker.addListener('dragend', eventDragEnd);
        document.getElementById('Lat').value = location.lat();
        document.getElementById('Lng').value = location.lng();
    }

    if (isInfo) {
        alertify.notify("Chọn tọa độ thành công, có thể đóng cửa sổ này và click 'Lưu thay đổi'", "success", 5);
    }
    if (isSetCenter) {
        map.setCenter(location);
    }
}