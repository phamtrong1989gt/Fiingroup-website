var connection = new signalR.HubConnectionBuilder().withUrl("/WebsiteHub/Message").configureLogging(signalR.LogLevel.Information).build();
connection.start().catch(err => console.error(err.toString()));
connection.on("ReceiveMessage", (message) => {
    notifyChrome({ title: message.content, icon: "/content/Home/images/logo.png", body: message.content, url: "/Admin/Manager/ContactManager" })
});

//function requestPermission() {
//    return new Promise(function (resolve, reject) {
//        const permissionResult = Notification.requestPermission(function (result) {
//            // Xử lý phiên bản cũ với callback.
//            resolve(result);
//        });

//        if (permissionResult) {
//            permissionResult.then(resolve, reject);
//        }
//    })
//        .then(function (permissionResult) {
//            if (permissionResult !== 'granted') {
//                throw new Error('Permission not granted.');
//            }
//        });
//}

//function subscribeUserToPush() {
//    return navigator.serviceWorker.register('service-worker.js')
//        .then(function (registration) {
//            var subscribeOptions = {
//                userVisibleOnly: true,
//                applicationServerKey: btoa(
//                    'BFSFi9uWgYpC8W37299apE4QnwiFC1R4Z9Lpy_Q9xV3pSvSw5SPA3hd4HCPnfOXaMYofanRDvNROIH0CDTITD6Y'
//                )
//            };

//            return registration.pushManager.subscribe(subscribeOptions);
//        })
//        .then(function (pushSubscription) {
//            console.log('PushSubscription: ', JSON.stringify(pushSubscription));
//            return pushSubscription;
//        });
//}

// request permission on page load
document.addEventListener('DOMContentLoaded', function () {
    if (!Notification) {
        alert('Máy tính chưa cài trình duyệt chrome.');
        return;
    }

    if (Notification.permission !== "granted")
        Notification.requestPermission();
});

function requestPermission() {
    if (Notification.permission !== "granted")
        Notification.requestPermission();
}

function notifyChrome(data) {
    if (Notification.permission !== "granted")
        Notification.requestPermission();
    else {
        var notification = new Notification(data.title, {
            icon: data.icon,
            body: data.body,
        });

        notification.onclick = function () {
            window.open(data.url);
        };
    }
}