Date.prototype.format = function (format) {
    /*
     * eg:format="YYYY-MM-dd hh:mm:ss";

     */
    var o = {
        "M+": this.getMonth() + 1, // month
        "d+": this.getDate(), // day
        "h+": this.getHours(), // hour
        "m+": this.getMinutes(), // minute
        "s+": this.getSeconds(), // second
        "q+": Math.floor((this.getMonth() + 3) / 3), // quarter
        "S": this.getMilliseconds()
        // millisecond
    }
    if (/(y+)/.test(format)) {
        format = format.replace(RegExp.$1, (this.getFullYear() + "")
            .substr(4 - RegExp.$1.length));
    }
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(format)) {
            format = format.replace(RegExp.$1, RegExp.$1.length == 1 ? o[k]
                : ("00" + o[k]).substr(("" + o[k]).length));
        }
    }
    return format;
}
var mainPanel_maxRow = 30;
function displayMessage(message, type) {
    var color = null;
    switch (type)
    {
        case 2://经过
            color = "#C0A83A";
            break;
        case 3://错误
            color = "#FF0505";
            break;
        default://普通
            color = "#000000";
            break;
    }

    var childdiv = $('<div class="childdiv" style="color:' + color+'">' + (new Date()).format("yyyy-MM-dd hh:mm:ss") + '---' + message + '</div>');

    var childList = mbDisplay.children();
    if (childList.length <= 0) {
        childdiv.appendTo(mbDisplay);
    }
    else
    {
        childdiv.prependTo(mbDisplay);
    }

    childList = mbDisplay.children();

    while (childList.length > mainPanel_maxRow)
    {
        childList[childList.length-1].remove();
        childList = mbDisplay.children();
    }
}

var action_intervalCheckRunning = 1000;
var action_intervalAction = null;
var action_timer = null;
function action_AutoRunning() {
    if (action_intervalAction != null)
    {
        try {
            action_intervalAction();
        }
        catch (err)
        {
            displayMessage(err, 3);
        }
    }
    action_timer = setTimeout(action_AutoRunning, action_intervalCheckRunning);
}