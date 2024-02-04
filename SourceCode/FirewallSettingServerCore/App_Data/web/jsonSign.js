

 function stringIsNullOrEmpty(str)
 {
	 return str==null || str=="";
}
function signValue(name,security,tick,ip) {

    var secPart = security.split('|');//切割出多段key
    var curSecurity = null;
    var val = null;
    var sign = "";
    for (var i = 0; i < secPart.length; i++) {
        if (secPart[i] == null || secPart[i] == "") {
            continue;
        }
        val = "name=" + encodeURIComponent(name) + "&secret=" + secPart[i] + "&tick=" + tick + "&ip=" + ip ;//明文数据
        curSecurity = $.sha1(val);//散列
        sign += curSecurity;
        if (i < (secPart.length - 1)) {
            sign += "|";
        }
    }
    return sign;
}