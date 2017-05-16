var charts = {};
var data = {};

$(document).ready(function () {
    var dataHub = $.connection.dataHub;

    // Registering the function that is called by SignalR
    dataHub.client.updateStream = function (id, timeStamp, value) {
        if (id in data)
        {
            if (data[id].length != null){
                data[id].push(
                    {
                        timeStamp: (new Date(timeStamp)).toISOString(),
                        value: value
                    });
            }
            else {
                data[id] = [{
                    timeStamp: (new Date(timeStamp)).toISOString(),
                    value: value}]
            }

            if (data[id].length != null) {
                if (data[id].length >= $("#chart-" + id).data("size"))
                    data[id].splice(0, 1);
            }

            if (id in charts) {
                charts[id].redrawChart();
            }
        }
    };

    $.connection.hub.start();

    // Adding the chart once the use is done
    $("#modalAddButton").click(function () {
        // Post returns the PartialView for the new div
        $.post('Home/AddDataStream', {
            name: $("#streamName").val(),
            connectionString: $("#serviceProvider").val(),
            source: $("#connectionString").val(),
            dataType: $("#dataType").val(), 
            bufferSize: $("#bufferSize").val(),
            bufferType: $("#bufferType").val()
        },
            function (returnedData) {
                var newStream = $(returnedData);
                var canvas = $(newStream).find("[id^='chart-']");
                var dataElement = $(newStream).find("[id^='data-']");
                var id = canvas.data("stream-id");
                data[id] = getBufferedValues(canvas.data("buffered"));
                canvas.removeAttr("data-buffered");
                charts[id] = initChart(id, canvas[0]);
                // Fancying up...
                newStream.hide();
                $('#contentArea').append(newStream);
                //Using slow just to give time to data flow in
                newStream.slideDown("slow"); 

                // Clearing the form
                $('#streamForm').each(function () {
                    this.reset();
                });

        }).fail(function () {
            console.log("error");
        });

        $("#streamConfigModal").modal("hide");
    });

    // Firing up the modal once the + button is clicked
    $("#addChart").click(function () {
        $("#streamConfigModal").modal({ backdrop: "static" });
    });

    // Lazy Loading data
    $('#contentArea').load('Home/LoadStreams', LoadCharts);

    $('#startAllStreams').click(function () {
        $.ajax({
            type: "POST",
            url: 'Home/StartAllStreams'
        });
    });

    $('#stopAllStreams').click(function () {
        $.ajax({
            type: "POST",
            url: 'Home/StopAllStreams'
        });
    });

    $('#saveView').click(function () {
        $("#saveConfig").modal({ backdrop: "static" });
    });

    $('#saveConfigButton').click(function () {
        $.ajax({
            type: "POST",
            data: { name: $("#configName").val() },
            url: 'Home/SaveConfiguration'
        });

        $("#saveConfig").modal("hide");

        $('#saveForm').each(function () {
            this.reset();
        });
    });

    $('#loadConfiguredViews').click(function () {
        $('#configurations').load('Home/GetAllConfigurations')
    });
});

// Initialize the 3D chart
function initChart(id, canvas) {
    var margin = { top: 0, right: 0, bottom: 20, left: 20 },
        width = 960 - margin.left - margin.right,
        height = 250 - margin.top - margin.bottom;

    var format = d3.utcParse("%Y-%m-%dT%H:%M:%S.%L%Z");
    var freqFunc = function (d) { return d.value }
    var dateFunc = function (d) { return format(d.timeStamp) }

    var svgElement = d3.select(canvas).append("svg")
        .attr("width", width)
        .attr("height", height),
        pathClass = "path";
    var xScale, yScale, xAxisGen, yAxisGen, lineFunc;

    drawLineChart();

    function setChartParameters() {
        xScale = d3.scaleTime()
            .range([margin.left+10, width])
            .domain(d3.extent(data[id], dateFunc));

        yScale = d3.scaleLinear()
            .range([height-20, 0])
            .domain(d3.extent(data[id], freqFunc));

        xAxisGen = d3.axisBottom()
            .scale(xScale)
            .ticks(data[id].length);

        yAxisGen = d3.axisLeft()
            .scale(yScale)
            .ticks(5);

        lineFunc = d3.line()
            .x(function (d) {
                return xScale(format(d.timeStamp));
            })
            .y(function (d) {
                return yScale(d.value);
            });
    }

    function drawLineChart() {
        setChartParameters();

        svgElement.append("g")
            .attr("class", "x axis")
            .attr("transform", "translate(-5," + (height - margin.bottom) + ")")
            .call(xAxisGen);

        svgElement.append("g")
            .attr("class", "y axis")
            .attr("transform", "translate("+(margin.left+5)+",0)")
            .call(yAxisGen);

        svgElement.append("path")
            .attr("d", lineFunc(data[id]))
            .attr("stroke", "white")
            .attr("stroke-width", 1)
            .attr("fill", "none")
            .attr("class", pathClass);
    }

    function redrawLineChart() {

        setChartParameters();
        svgElement.selectAll("g.y.axis").call(yAxisGen);
        svgElement.selectAll("g.x.axis").call(xAxisGen);
        svgElement.selectAll("." + pathClass)
            .attr("d", lineFunc(data[id]));
    }

    // Returns the function to be updated later
    return {
        redrawChart: redrawLineChart
    }
}

function LoadCharts (response, status, hxr) {
    if (status == "error") {
        // Clear content and pop up error
        alert('error');
    }
    else {
        // Creating the charts and connecting to SignalR
        $("[id^='chart-']").each(function () {
            var id = $(this).data("stream-id");
            data[id] = getBufferedValues($(this).data("buffered"));
            $(this).removeAttr("data-buffered");
            charts[id] = initChart(id, $(this)[0]);
        })
    }
}

function loadConfiguration(configuration)
{
    $('#contentArea').load('Home/LoadConfiguration',
        { name: configuration },
        LoadCharts);
}

function getBufferedValues(bufferedData) {
    var data = [];
    for (var i = 0; i < bufferedData.length; i++) {
        data.push({
            timeStamp: (new Date(bufferedData[i].FormatedTimeStamp)).toISOString(),
            value: bufferedData[i].Value
        });
    }
    return data;
}

function removeChart(id) {
    $("#row-" + id).slideUp("slow", function () {
        $("#row-" + id).hide();
        $("#row-" + id).remove();
        stopListening(id);
        delete charts[id];
        delete data[id];
    })
}

function stopListening(id){
    $.ajax({
        type: "POST",
        url: 'Home/StopDataStream',
        data: { id: id }
    });
}

function startListening(id) {
    $.ajax({
        type: "POST",
        url: 'Home/StartDataStream',
        data: { id: id }
    });
}