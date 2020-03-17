// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

let isTouching = false;
let heartRate = 0.0;
let heartRateMax = 0.0;
let gsrMax = 0.0;

// Write your Javascript code.
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

        if (data.type == "devices" || data.type == "deviceChanged") {
            $("#devicesInfo").html(JSON.stringify(data, null, 2));
        }
        else if (data.type === "mouseMetrics") {
            $("#mouseMetrics").html(JSON.stringify(data, null, 2));
        }
        else if (data.type === "bioMetrics") {
            $("#bioMetrics").html(JSON.stringify(data, null, 2));

            $("#signalQuality").html(data["heartRateState"]);

            heartRate = data["heartRate"];
            $("#hRate").html(heartRate);

            $("#avgHRate").html(JSON.stringify(data["heartRateAvg"]));

            heartRateMax = data["heartRateMax"];
            $("#maxHRate").html(heartRateMax);

            let gsrRaw = data["gsr"];

            if (gsrRaw > gsrMax) {
                gsrMax = gsrRaw;
                $("#gsrRawMax").html(data["gsr"].toFixed(2));
            }

            $("#gsrRaw").html(data["gsr"].toFixed(2));
        }
        else if (data.type === "bioRaw") {
            if (data["touch"] == false) {
                isTouching = false;
                console.log("User is not touching the mouse." + data["touch"]);
            } else {
                isTouching = true;
                console.log("User is touching the mouse." + data["touch"]);
            }

            $("#bioRaw").html(JSON.stringify(data, null, 2));
            $("#hRateRaw").html(JSON.stringify(data["heartRate"]));
        }
    };
});

var canvas = document.getElementById('myChart');
var data = {
    labels: [0, 1, 2, 3, 4, 5, 6],
    datasets: [
        {
            label: "BPM",
            fill: true,
            lineTension: 0.0,
            backgroundColor: "rgba(75,192,192,0.4)",
            borderColor: "rgba(75,192,192,1)",
            borderCapStyle: 'butt',
            borderDash: [],
            borderDashOffset: 0.0,
            borderJoinStyle: 'miter',
            pointBorderColor: "rgba(75,192,192,1)",
            pointBackgroundColor: "#fff",
            pointBorderWidth: 1,
            pointHoverRadius: 5,
            pointHoverBackgroundColor: "rgba(75,192,192,1)",
            pointHoverBorderColor: "rgba(220,220,220,1)",
            pointHoverBorderWidth: 2,
            pointRadius: 5,
            pointHitRadius: 10,
            data: [heartRate],
        }
    ]
};

var zero = 7;

function adddata() {

    var value = heartRate;
    //x-axis (secs)
    myLineChart.data.labels.push(zero);
    myLineChart.data.labels.splice(0, 1);

    //y-axis
    myLineChart.data.datasets[0].data.splice(0, 0, value);
    console.log(myLineChart.data.datasets[0].data);
    //myLineChart.data.datasets[0].data.push(value);

    myLineChart.update();
    zero++;
}

if (isTouching) {
    setInterval(function () {
        adddata();
    }, 600);
}
else {

}

var option = {
    showLines: true,
    scales: {
        yAxes: [{
            display: true,
            ticks: {
                beginAtZero: false,
                min: 0,
                max: 200
            }
        }]
    }
};
var myLineChart = Chart.Line(canvas, {
    data: data,
    options: option
});





