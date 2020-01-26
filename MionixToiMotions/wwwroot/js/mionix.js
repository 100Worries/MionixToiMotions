$(function () {
    var WebSocket = window.WebSocket || window.MozWebSocket;
    var mionixSocket = new WebSocket("ws://localhost:7681", "mionix-beta");

    mionixSocket.onopen = function () {
        $("#connectLabel").addClass("label-success");
        $("#connectLabel").text("Connected");
        mionixSocket.send(JSON.stringify({ type: 'getDevices' }));
    };

    mionixSocket.onerror = function () {
        $("#connectLabel").addClass("label-danger");
        $("#connectLabel").text("Not connected");
    };

    mionixSocket.onmessage = function (message) {
        var data = JSON.parse(message.data);
        if (data.type == "devices" || data.type == "deviceChanged")
            $("#devicesInfo").html(JSON.stringify(data, null, 2));
        else if (data.type == "mouseMetrics")
            $("#mouseMetrics").html(JSON.stringify(data, null, 2));
        else if (data.type == "bioMetrics")
            $("#bioMetrics").html(JSON.stringify(data, null, 2));
        else if (data.type == "bioRaw")
            $("#bioRaw").html(JSON.stringify(data, null, 2));
    };
});