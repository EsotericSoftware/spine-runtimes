var Ajax = {

    get: function (url, callback) {
        var http = undefined;


        if (typeof ActiveXObject != 'undefined') {
            try { http = new ActiveXObject("Msxml2.XMLHTTP"); }
            catch (e) {
                try { http = new ActiveXObject("Microsoft.XMLHTTP"); }
                catch (E) { http = undefined; }
            }
        } else if (XMLHttpRequest) {
            try { http = new XMLHttpRequest(); }
            catch (e) { http = false; }
        }

        if (!http || !url) return;
        if (http.overrideMimeType) http.overrideMimeType('text/xml');


        //avoid cache
        var now = "uid=" + Date.now();
        url += (url.indexOf("?") + 1) ? "&" : "?";
        url += now;

        http.open("GET", url, true);
        var local = (url.indexOf('http') != 0);
        http.onreadystatechange = function () {
            if (http.readyState == 4) {
                if (http.status == 200 || (local && http.responseText != '')) {
                    var result = "";
                    if (http.responseText) result = http.responseText;


                    if (callback) callback(result);
                } else {

                    console.log('Error ', http.status);
                }
            }
        }
        http.send(null);
    }
}