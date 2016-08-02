var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var AssetManager = (function () {
            function AssetManager() {
                this._assets = {};
                this._errors = {};
                this._toLoad = 0;
                this._loaded = 0;
            }
            AssetManager.prototype.loadText = function (path, success, error) {
                var _this = this;
                this._toLoad++;
                var request = new XMLHttpRequest();
                request.onreadystatechange = function () {
                    if (request.readyState == XMLHttpRequest.DONE) {
                        if (request.status >= 200 && request.status < 300) {
                            if (success)
                                success(path, request.responseText);
                            _this._assets[path] = request.responseText;
                        }
                        else {
                            if (error)
                                error(path, "Couldn't load text " + path + ": status " + request.status + ", " + request.responseBody);
                            _this._errors[path] = "Couldn't load text " + path + ": status " + request.status + ", " + request.responseBody;
                        }
                        _this._toLoad--;
                        _this._loaded++;
                    }
                };
                request.open("GET", path, true);
                request.send();
            };
            AssetManager.prototype.loadTexture = function (path, success, error) {
                var _this = this;
                this._toLoad++;
                var img = new Image();
                img.src = path;
                img.onload = function (ev) {
                    if (success)
                        success(path, img);
                    var texture = new webgl.Texture(img);
                    _this._assets[path] = texture;
                    _this._toLoad--;
                    _this._loaded++;
                };
                img.onerror = function (ev) {
                    _this._errors[path] = "Couldn't load image " + path;
                    _this._toLoad--;
                    _this._loaded++;
                };
            };
            AssetManager.prototype.get = function (path) {
                return this._assets[path];
            };
            AssetManager.prototype.remove = function (path) {
                var asset = this._assets[path];
                this._assets[path] = null;
            };
            AssetManager.prototype.removeAll = function () {
                this._assets = {};
            };
            AssetManager.prototype.isLoadingComplete = function () {
                return this._toLoad == 0;
            };
            AssetManager.prototype.toLoad = function () {
                return this._toLoad;
            };
            AssetManager.prototype.loaded = function () {
                return this._loaded;
            };
            return AssetManager;
        }());
        webgl.AssetManager = AssetManager;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        webgl.M00 = 0;
        webgl.M01 = 4;
        webgl.M02 = 8;
        webgl.M03 = 12;
        webgl.M10 = 1;
        webgl.M11 = 5;
        webgl.M12 = 9;
        webgl.M13 = 13;
        webgl.M20 = 2;
        webgl.M21 = 6;
        webgl.M22 = 10;
        webgl.M23 = 14;
        webgl.M30 = 3;
        webgl.M31 = 7;
        webgl.M32 = 11;
        webgl.M33 = 15;
        var Matrix4 = (function () {
            function Matrix4() {
                this.temp = new Float32Array(16);
                this.values = new Float32Array(16);
                this.values[webgl.M00] = 1;
                this.values[webgl.M11] = 1;
                this.values[webgl.M22] = 1;
                this.values[webgl.M33] = 1;
            }
            Matrix4.prototype.set = function (values) {
                this.values.set(values);
                return this;
            };
            Matrix4.prototype.transpose = function () {
                this.temp[webgl.M00] = this.values[webgl.M00];
                this.temp[webgl.M01] = this.values[webgl.M10];
                this.temp[webgl.M02] = this.values[webgl.M20];
                this.temp[webgl.M03] = this.values[webgl.M30];
                this.temp[webgl.M10] = this.values[webgl.M01];
                this.temp[webgl.M11] = this.values[webgl.M11];
                this.temp[webgl.M12] = this.values[webgl.M21];
                this.temp[webgl.M13] = this.values[webgl.M31];
                this.temp[webgl.M20] = this.values[webgl.M02];
                this.temp[webgl.M21] = this.values[webgl.M12];
                this.temp[webgl.M22] = this.values[webgl.M22];
                this.temp[webgl.M23] = this.values[webgl.M32];
                this.temp[webgl.M30] = this.values[webgl.M03];
                this.temp[webgl.M31] = this.values[webgl.M13];
                this.temp[webgl.M32] = this.values[webgl.M23];
                this.temp[webgl.M33] = this.values[webgl.M33];
                return this.set(this.temp);
            };
            Matrix4.prototype.identity = function () {
                this.values[webgl.M00] = 1;
                this.values[webgl.M01] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M03] = 0;
                this.values[webgl.M10] = 0;
                this.values[webgl.M11] = 1;
                this.values[webgl.M12] = 0;
                this.values[webgl.M13] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M21] = 0;
                this.values[webgl.M22] = 1;
                this.values[webgl.M23] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M32] = 0;
                this.values[webgl.M33] = 1;
                return this;
            };
            Matrix4.prototype.invert = function () {
                var l_det = this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M11]
                    * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M10] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M10]
                    * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M13] + this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M02] * this.values[webgl.M13]
                    + this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M32]
                    * this.values[webgl.M13] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M13] + this.values[webgl.M30] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M10] * this.values[webgl.M31]
                    * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M10]
                    * this.values[webgl.M01] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M33]
                    + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M33] + this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M12]
                    * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
                if (l_det == 0)
                    throw new Error("non-invertible matrix");
                var inv_det = 1.0 / l_det;
                this.temp[webgl.M00] = this.values[webgl.M12] * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M13] * this.values[webgl.M22] * this.values[webgl.M31] + this.values[webgl.M13] * this.values[webgl.M21] * this.values[webgl.M32] - this.values[webgl.M11]
                    * this.values[webgl.M23] * this.values[webgl.M32] - this.values[webgl.M12] * this.values[webgl.M21] * this.values[webgl.M33] + this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M01] = this.values[webgl.M03] * this.values[webgl.M22] * this.values[webgl.M31] - this.values[webgl.M02] * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M03] * this.values[webgl.M21] * this.values[webgl.M32] + this.values[webgl.M01]
                    * this.values[webgl.M23] * this.values[webgl.M32] + this.values[webgl.M02] * this.values[webgl.M21] * this.values[webgl.M33] - this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M02] = this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M31] - this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M31] + this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M32] - this.values[webgl.M01]
                    * this.values[webgl.M13] * this.values[webgl.M32] - this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M33] + this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33];
                this.temp[webgl.M03] = this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M21] - this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M21] - this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M22] + this.values[webgl.M01]
                    * this.values[webgl.M13] * this.values[webgl.M22] + this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M23] - this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23];
                this.temp[webgl.M10] = this.values[webgl.M13] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M12] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M13] * this.values[webgl.M20] * this.values[webgl.M32] + this.values[webgl.M10]
                    * this.values[webgl.M23] * this.values[webgl.M32] + this.values[webgl.M12] * this.values[webgl.M20] * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M11] = this.values[webgl.M02] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M22] * this.values[webgl.M30] + this.values[webgl.M03] * this.values[webgl.M20] * this.values[webgl.M32] - this.values[webgl.M00]
                    * this.values[webgl.M23] * this.values[webgl.M32] - this.values[webgl.M02] * this.values[webgl.M20] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M22] * this.values[webgl.M33];
                this.temp[webgl.M12] = this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M32] + this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M32] + this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M12] * this.values[webgl.M33];
                this.temp[webgl.M13] = this.values[webgl.M02] * this.values[webgl.M13] * this.values[webgl.M20] - this.values[webgl.M03] * this.values[webgl.M12] * this.values[webgl.M20] + this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M22] - this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M22] - this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M12] * this.values[webgl.M23];
                this.temp[webgl.M20] = this.values[webgl.M11] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M13] * this.values[webgl.M21] * this.values[webgl.M30] + this.values[webgl.M13] * this.values[webgl.M20] * this.values[webgl.M31] - this.values[webgl.M10]
                    * this.values[webgl.M23] * this.values[webgl.M31] - this.values[webgl.M11] * this.values[webgl.M20] * this.values[webgl.M33] + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M33];
                this.temp[webgl.M21] = this.values[webgl.M03] * this.values[webgl.M21] * this.values[webgl.M30] - this.values[webgl.M01] * this.values[webgl.M23] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M20] * this.values[webgl.M31] + this.values[webgl.M00]
                    * this.values[webgl.M23] * this.values[webgl.M31] + this.values[webgl.M01] * this.values[webgl.M20] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M33];
                this.temp[webgl.M22] = this.values[webgl.M01] * this.values[webgl.M13] * this.values[webgl.M30] - this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M30] + this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M31] - this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M31] - this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M33];
                this.temp[webgl.M23] = this.values[webgl.M03] * this.values[webgl.M11] * this.values[webgl.M20] - this.values[webgl.M01] * this.values[webgl.M13] * this.values[webgl.M20] - this.values[webgl.M03] * this.values[webgl.M10] * this.values[webgl.M21] + this.values[webgl.M00]
                    * this.values[webgl.M13] * this.values[webgl.M21] + this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M23];
                this.temp[webgl.M30] = this.values[webgl.M12] * this.values[webgl.M21] * this.values[webgl.M30] - this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M12] * this.values[webgl.M20] * this.values[webgl.M31] + this.values[webgl.M10]
                    * this.values[webgl.M22] * this.values[webgl.M31] + this.values[webgl.M11] * this.values[webgl.M20] * this.values[webgl.M32] - this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M32];
                this.temp[webgl.M31] = this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M21] * this.values[webgl.M30] + this.values[webgl.M02] * this.values[webgl.M20] * this.values[webgl.M31] - this.values[webgl.M00]
                    * this.values[webgl.M22] * this.values[webgl.M31] - this.values[webgl.M01] * this.values[webgl.M20] * this.values[webgl.M32] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32];
                this.temp[webgl.M32] = this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M30] - this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M30] - this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M31] + this.values[webgl.M00]
                    * this.values[webgl.M12] * this.values[webgl.M31] + this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M32] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32];
                this.temp[webgl.M33] = this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M20] - this.values[webgl.M02] * this.values[webgl.M11] * this.values[webgl.M20] + this.values[webgl.M02] * this.values[webgl.M10] * this.values[webgl.M21] - this.values[webgl.M00]
                    * this.values[webgl.M12] * this.values[webgl.M21] - this.values[webgl.M01] * this.values[webgl.M10] * this.values[webgl.M22] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22];
                this.values[webgl.M00] = this.temp[webgl.M00] * inv_det;
                this.values[webgl.M01] = this.temp[webgl.M01] * inv_det;
                this.values[webgl.M02] = this.temp[webgl.M02] * inv_det;
                this.values[webgl.M03] = this.temp[webgl.M03] * inv_det;
                this.values[webgl.M10] = this.temp[webgl.M10] * inv_det;
                this.values[webgl.M11] = this.temp[webgl.M11] * inv_det;
                this.values[webgl.M12] = this.temp[webgl.M12] * inv_det;
                this.values[webgl.M13] = this.temp[webgl.M13] * inv_det;
                this.values[webgl.M20] = this.temp[webgl.M20] * inv_det;
                this.values[webgl.M21] = this.temp[webgl.M21] * inv_det;
                this.values[webgl.M22] = this.temp[webgl.M22] * inv_det;
                this.values[webgl.M23] = this.temp[webgl.M23] * inv_det;
                this.values[webgl.M30] = this.temp[webgl.M30] * inv_det;
                this.values[webgl.M31] = this.temp[webgl.M31] * inv_det;
                this.values[webgl.M32] = this.temp[webgl.M32] * inv_det;
                this.values[webgl.M33] = this.temp[webgl.M33] * inv_det;
                return this;
            };
            Matrix4.prototype.determinant = function () {
                return this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M11]
                    * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M10] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M03] + this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M10]
                    * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M03] - this.values[webgl.M30] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M13] + this.values[webgl.M20] * this.values[webgl.M31] * this.values[webgl.M02] * this.values[webgl.M13]
                    + this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M22] * this.values[webgl.M13] - this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M32]
                    * this.values[webgl.M13] + this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M32] * this.values[webgl.M13] + this.values[webgl.M30] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M10] * this.values[webgl.M31]
                    * this.values[webgl.M02] * this.values[webgl.M23] - this.values[webgl.M30] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M00] * this.values[webgl.M31] * this.values[webgl.M12] * this.values[webgl.M23] + this.values[webgl.M10]
                    * this.values[webgl.M01] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M32] * this.values[webgl.M23] - this.values[webgl.M20] * this.values[webgl.M11] * this.values[webgl.M02] * this.values[webgl.M33]
                    + this.values[webgl.M10] * this.values[webgl.M21] * this.values[webgl.M02] * this.values[webgl.M33] + this.values[webgl.M20] * this.values[webgl.M01] * this.values[webgl.M12] * this.values[webgl.M33] - this.values[webgl.M00] * this.values[webgl.M21] * this.values[webgl.M12]
                    * this.values[webgl.M33] - this.values[webgl.M10] * this.values[webgl.M01] * this.values[webgl.M22] * this.values[webgl.M33] + this.values[webgl.M00] * this.values[webgl.M11] * this.values[webgl.M22] * this.values[webgl.M33];
            };
            Matrix4.prototype.translate = function (x, y, z) {
                this.values[webgl.M03] += x;
                this.values[webgl.M13] += y;
                this.values[webgl.M23] += z;
                return this;
            };
            Matrix4.prototype.copy = function () {
                return new Matrix4().set(this.values);
            };
            Matrix4.prototype.projection = function (near, far, fovy, aspectRatio) {
                this.identity();
                var l_fd = (1.0 / Math.tan((fovy * (Math.PI / 180)) / 2.0));
                var l_a1 = (far + near) / (near - far);
                var l_a2 = (2 * far * near) / (near - far);
                this.values[webgl.M00] = l_fd / aspectRatio;
                this.values[webgl.M10] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M01] = 0;
                this.values[webgl.M11] = l_fd;
                this.values[webgl.M21] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M12] = 0;
                this.values[webgl.M22] = l_a1;
                this.values[webgl.M32] = -1;
                this.values[webgl.M03] = 0;
                this.values[webgl.M13] = 0;
                this.values[webgl.M23] = l_a2;
                this.values[webgl.M33] = 0;
                return this;
            };
            Matrix4.prototype.ortho2d = function (x, y, width, height) {
                return this.ortho(x, x + width, y, y + height, 0, 1);
            };
            Matrix4.prototype.ortho = function (left, right, bottom, top, near, far) {
                this.identity();
                var x_orth = 2 / (right - left);
                var y_orth = 2 / (top - bottom);
                var z_orth = -2 / (far - near);
                var tx = -(right + left) / (right - left);
                var ty = -(top + bottom) / (top - bottom);
                var tz = -(far + near) / (far - near);
                this.values[webgl.M00] = x_orth;
                this.values[webgl.M10] = 0;
                this.values[webgl.M20] = 0;
                this.values[webgl.M30] = 0;
                this.values[webgl.M01] = 0;
                this.values[webgl.M11] = y_orth;
                this.values[webgl.M21] = 0;
                this.values[webgl.M31] = 0;
                this.values[webgl.M02] = 0;
                this.values[webgl.M12] = 0;
                this.values[webgl.M22] = z_orth;
                this.values[webgl.M32] = 0;
                this.values[webgl.M03] = tx;
                this.values[webgl.M13] = ty;
                this.values[webgl.M23] = tz;
                this.values[webgl.M33] = 1;
                return this;
            };
            Matrix4.prototype.multiply = function (matrix) {
                this.temp[webgl.M00] = this.values[webgl.M00] * matrix.values[webgl.M00] + this.values[webgl.M01] * matrix.values[webgl.M10] + this.values[webgl.M02] * matrix.values[webgl.M20] + this.values[webgl.M03]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M01] = this.values[webgl.M00] * matrix.values[webgl.M01] + this.values[webgl.M01] * matrix.values[webgl.M11] + this.values[webgl.M02] * matrix.values[webgl.M21] + this.values[webgl.M03]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M02] = this.values[webgl.M00] * matrix.values[webgl.M02] + this.values[webgl.M01] * matrix.values[webgl.M12] + this.values[webgl.M02] * matrix.values[webgl.M22] + this.values[webgl.M03]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M03] = this.values[webgl.M00] * matrix.values[webgl.M03] + this.values[webgl.M01] * matrix.values[webgl.M13] + this.values[webgl.M02] * matrix.values[webgl.M23] + this.values[webgl.M03]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M10] = this.values[webgl.M10] * matrix.values[webgl.M00] + this.values[webgl.M11] * matrix.values[webgl.M10] + this.values[webgl.M12] * matrix.values[webgl.M20] + this.values[webgl.M13]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M11] = this.values[webgl.M10] * matrix.values[webgl.M01] + this.values[webgl.M11] * matrix.values[webgl.M11] + this.values[webgl.M12] * matrix.values[webgl.M21] + this.values[webgl.M13]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M12] = this.values[webgl.M10] * matrix.values[webgl.M02] + this.values[webgl.M11] * matrix.values[webgl.M12] + this.values[webgl.M12] * matrix.values[webgl.M22] + this.values[webgl.M13]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M13] = this.values[webgl.M10] * matrix.values[webgl.M03] + this.values[webgl.M11] * matrix.values[webgl.M13] + this.values[webgl.M12] * matrix.values[webgl.M23] + this.values[webgl.M13]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M20] = this.values[webgl.M20] * matrix.values[webgl.M00] + this.values[webgl.M21] * matrix.values[webgl.M10] + this.values[webgl.M22] * matrix.values[webgl.M20] + this.values[webgl.M23]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M21] = this.values[webgl.M20] * matrix.values[webgl.M01] + this.values[webgl.M21] * matrix.values[webgl.M11] + this.values[webgl.M22] * matrix.values[webgl.M21] + this.values[webgl.M23]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M22] = this.values[webgl.M20] * matrix.values[webgl.M02] + this.values[webgl.M21] * matrix.values[webgl.M12] + this.values[webgl.M22] * matrix.values[webgl.M22] + this.values[webgl.M23]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M23] = this.values[webgl.M20] * matrix.values[webgl.M03] + this.values[webgl.M21] * matrix.values[webgl.M13] + this.values[webgl.M22] * matrix.values[webgl.M23] + this.values[webgl.M23]
                    * matrix.values[webgl.M33];
                this.temp[webgl.M30] = this.values[webgl.M30] * matrix.values[webgl.M00] + this.values[webgl.M31] * matrix.values[webgl.M10] + this.values[webgl.M32] * matrix.values[webgl.M20] + this.values[webgl.M33]
                    * matrix.values[webgl.M30];
                this.temp[webgl.M31] = this.values[webgl.M30] * matrix.values[webgl.M01] + this.values[webgl.M31] * matrix.values[webgl.M11] + this.values[webgl.M32] * matrix.values[webgl.M21] + this.values[webgl.M33]
                    * matrix.values[webgl.M31];
                this.temp[webgl.M32] = this.values[webgl.M30] * matrix.values[webgl.M02] + this.values[webgl.M31] * matrix.values[webgl.M12] + this.values[webgl.M32] * matrix.values[webgl.M22] + this.values[webgl.M33]
                    * matrix.values[webgl.M32];
                this.temp[webgl.M33] = this.values[webgl.M30] * matrix.values[webgl.M03] + this.values[webgl.M31] * matrix.values[webgl.M13] + this.values[webgl.M32] * matrix.values[webgl.M23] + this.values[webgl.M33]
                    * matrix.values[webgl.M33];
                return this.set(this.temp);
            };
            Matrix4.prototype.multiplyLeft = function (matrix) {
                this.temp[webgl.M00] = matrix.values[webgl.M00] * this.values[webgl.M00] + matrix.values[webgl.M01] * this.values[webgl.M10] + matrix.values[webgl.M02] * this.values[webgl.M20] + matrix.values[webgl.M03]
                    * this.values[webgl.M30];
                this.temp[webgl.M01] = matrix.values[webgl.M00] * this.values[webgl.M01] + matrix.values[webgl.M01] * this.values[webgl.M11] + matrix.values[webgl.M02] * this.values[webgl.M21] + matrix.values[webgl.M03]
                    * this.values[webgl.M31];
                this.temp[webgl.M02] = matrix.values[webgl.M00] * this.values[webgl.M02] + matrix.values[webgl.M01] * this.values[webgl.M12] + matrix.values[webgl.M02] * this.values[webgl.M22] + matrix.values[webgl.M03]
                    * this.values[webgl.M32];
                this.temp[webgl.M03] = matrix.values[webgl.M00] * this.values[webgl.M03] + matrix.values[webgl.M01] * this.values[webgl.M13] + matrix.values[webgl.M02] * this.values[webgl.M23] + matrix.values[webgl.M03]
                    * this.values[webgl.M33];
                this.temp[webgl.M10] = matrix.values[webgl.M10] * this.values[webgl.M00] + matrix.values[webgl.M11] * this.values[webgl.M10] + matrix.values[webgl.M12] * this.values[webgl.M20] + matrix.values[webgl.M13]
                    * this.values[webgl.M30];
                this.temp[webgl.M11] = matrix.values[webgl.M10] * this.values[webgl.M01] + matrix.values[webgl.M11] * this.values[webgl.M11] + matrix.values[webgl.M12] * this.values[webgl.M21] + matrix.values[webgl.M13]
                    * this.values[webgl.M31];
                this.temp[webgl.M12] = matrix.values[webgl.M10] * this.values[webgl.M02] + matrix.values[webgl.M11] * this.values[webgl.M12] + matrix.values[webgl.M12] * this.values[webgl.M22] + matrix.values[webgl.M13]
                    * this.values[webgl.M32];
                this.temp[webgl.M13] = matrix.values[webgl.M10] * this.values[webgl.M03] + matrix.values[webgl.M11] * this.values[webgl.M13] + matrix.values[webgl.M12] * this.values[webgl.M23] + matrix.values[webgl.M13]
                    * this.values[webgl.M33];
                this.temp[webgl.M20] = matrix.values[webgl.M20] * this.values[webgl.M00] + matrix.values[webgl.M21] * this.values[webgl.M10] + matrix.values[webgl.M22] * this.values[webgl.M20] + matrix.values[webgl.M23]
                    * this.values[webgl.M30];
                this.temp[webgl.M21] = matrix.values[webgl.M20] * this.values[webgl.M01] + matrix.values[webgl.M21] * this.values[webgl.M11] + matrix.values[webgl.M22] * this.values[webgl.M21] + matrix.values[webgl.M23]
                    * this.values[webgl.M31];
                this.temp[webgl.M22] = matrix.values[webgl.M20] * this.values[webgl.M02] + matrix.values[webgl.M21] * this.values[webgl.M12] + matrix.values[webgl.M22] * this.values[webgl.M22] + matrix.values[webgl.M23]
                    * this.values[webgl.M32];
                this.temp[webgl.M23] = matrix.values[webgl.M20] * this.values[webgl.M03] + matrix.values[webgl.M21] * this.values[webgl.M13] + matrix.values[webgl.M22] * this.values[webgl.M23] + matrix.values[webgl.M23]
                    * this.values[webgl.M33];
                this.temp[webgl.M30] = matrix.values[webgl.M30] * this.values[webgl.M00] + matrix.values[webgl.M31] * this.values[webgl.M10] + matrix.values[webgl.M32] * this.values[webgl.M20] + matrix.values[webgl.M33]
                    * this.values[webgl.M30];
                this.temp[webgl.M31] = matrix.values[webgl.M30] * this.values[webgl.M01] + matrix.values[webgl.M31] * this.values[webgl.M11] + matrix.values[webgl.M32] * this.values[webgl.M21] + matrix.values[webgl.M33]
                    * this.values[webgl.M31];
                this.temp[webgl.M32] = matrix.values[webgl.M30] * this.values[webgl.M02] + matrix.values[webgl.M31] * this.values[webgl.M12] + matrix.values[webgl.M32] * this.values[webgl.M22] + matrix.values[webgl.M33]
                    * this.values[webgl.M32];
                this.temp[webgl.M33] = matrix.values[webgl.M30] * this.values[webgl.M03] + matrix.values[webgl.M31] * this.values[webgl.M13] + matrix.values[webgl.M32] * this.values[webgl.M23] + matrix.values[webgl.M33]
                    * this.values[webgl.M33];
                return this.set(this.temp);
            };
            return Matrix4;
        }());
        webgl.Matrix4 = Matrix4;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
};
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Mesh = (function () {
            function Mesh(_attributes, maxVertices, maxIndices) {
                this._attributes = _attributes;
                this._numVertices = 0;
                this._dirtyVertices = false;
                this._numIndices = 0;
                this._dirtyIndices = false;
                this._elementsPerVertex = 0;
                this._elementsPerVertex = 0;
                for (var i = 0; i < _attributes.length; i++) {
                    this._elementsPerVertex += _attributes[i].numElements;
                }
                this._vertices = new Float32Array(maxVertices * this._elementsPerVertex);
                this._indices = new Uint16Array(maxIndices);
            }
            Mesh.prototype.attributes = function () { return this._attributes; };
            Mesh.prototype.maxVertices = function () { return this._vertices.length / this._elementsPerVertex; };
            Mesh.prototype.numVertices = function () { return this._numVertices / this._elementsPerVertex; };
            Mesh.prototype.maxIndices = function () { return this._indices.length; };
            Mesh.prototype.numIndices = function () { return this._numIndices; };
            Mesh.prototype.setVertices = function (vertices) {
                this._dirtyVertices = true;
                if (vertices.length > this._vertices.length)
                    throw Error("Mesh can't store more than " + this.maxVertices() + " vertices");
                this._vertices.set(vertices, 0);
                this._numVertices = vertices.length;
            };
            Mesh.prototype.setIndices = function (indices) {
                this._dirtyIndices = true;
                if (indices.length > this._indices.length)
                    throw Error("Mesh can't store more than " + this.maxIndices() + " indices");
                this._indices.set(indices, 0);
                this._numIndices = indices.length;
            };
            Mesh.prototype.render = function (shader, primitiveType) {
                this.renderWithOffset(shader, primitiveType, 0, this._numIndices > 0 ? this._numIndices : this._numVertices);
            };
            Mesh.prototype.renderWithOffset = function (shader, primitiveType, offset, count) {
                if (this._dirtyVertices || this._dirtyIndices)
                    this.update();
                this.bind(shader);
                if (this._numIndices > 0)
                    webgl.gl.drawElements(primitiveType, count, webgl.gl.UNSIGNED_SHORT, offset * 2);
                else
                    webgl.gl.drawArrays(primitiveType, offset, count);
                this.unbind(shader);
            };
            Mesh.prototype.bind = function (shader) {
                webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, this._verticesBuffer);
                var offset = 0;
                for (var i = 0; i < this._attributes.length; i++) {
                    var attrib = this._attributes[i];
                    var location_1 = shader.getAttributeLocation(attrib.name);
                    webgl.gl.enableVertexAttribArray(location_1);
                    webgl.gl.vertexAttribPointer(location_1, attrib.numElements, webgl.gl.FLOAT, false, this._elementsPerVertex * 4, offset * 4);
                    offset += attrib.numElements;
                }
                if (this._numIndices > 0)
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indicesBuffer);
            };
            Mesh.prototype.unbind = function (shader) {
                for (var i = 0; i < this._attributes.length; i++) {
                    var attrib = this._attributes[i];
                    var location_2 = shader.getAttributeLocation(attrib.name);
                    webgl.gl.disableVertexAttribArray(location_2);
                }
                webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, null);
                if (this._numIndices > 0)
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, null);
            };
            Mesh.prototype.update = function () {
                if (this._dirtyVertices) {
                    if (!this._verticesBuffer) {
                        this._verticesBuffer = webgl.gl.createBuffer();
                    }
                    webgl.gl.bindBuffer(webgl.gl.ARRAY_BUFFER, this._verticesBuffer);
                    webgl.gl.bufferData(webgl.gl.ARRAY_BUFFER, this._vertices.subarray(0, this._numVertices), webgl.gl.STATIC_DRAW);
                    this._dirtyVertices = false;
                }
                if (this._dirtyIndices) {
                    if (!this._indicesBuffer) {
                        this._indicesBuffer = webgl.gl.createBuffer();
                    }
                    webgl.gl.bindBuffer(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indicesBuffer);
                    webgl.gl.bufferData(webgl.gl.ELEMENT_ARRAY_BUFFER, this._indices.subarray(0, this._numIndices), webgl.gl.STATIC_DRAW);
                    this._dirtyIndices = false;
                }
            };
            return Mesh;
        }());
        webgl.Mesh = Mesh;
        var VertexAttribute = (function () {
            function VertexAttribute(name, type, numElements) {
                this.name = name;
                this.type = type;
                this.numElements = numElements;
            }
            return VertexAttribute;
        }());
        webgl.VertexAttribute = VertexAttribute;
        var Position2Attribute = (function (_super) {
            __extends(Position2Attribute, _super);
            function Position2Attribute() {
                _super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 2);
            }
            return Position2Attribute;
        }(VertexAttribute));
        webgl.Position2Attribute = Position2Attribute;
        var Position3Attribute = (function (_super) {
            __extends(Position3Attribute, _super);
            function Position3Attribute() {
                _super.call(this, webgl.Shader.POSITION, VertexAttributeType.Float, 3);
            }
            return Position3Attribute;
        }(VertexAttribute));
        webgl.Position3Attribute = Position3Attribute;
        var TexCoordAttribute = (function (_super) {
            __extends(TexCoordAttribute, _super);
            function TexCoordAttribute(unit) {
                if (unit === void 0) { unit = 0; }
                _super.call(this, webgl.Shader.TEXCOORDS + (unit == 0 ? "" : unit), VertexAttributeType.Float, 2);
            }
            return TexCoordAttribute;
        }(VertexAttribute));
        webgl.TexCoordAttribute = TexCoordAttribute;
        var ColorAttribute = (function (_super) {
            __extends(ColorAttribute, _super);
            function ColorAttribute() {
                _super.call(this, webgl.Shader.COLOR, VertexAttributeType.Float, 4);
            }
            return ColorAttribute;
        }(VertexAttribute));
        webgl.ColorAttribute = ColorAttribute;
        (function (VertexAttributeType) {
            VertexAttributeType[VertexAttributeType["Float"] = 0] = "Float";
        })(webgl.VertexAttributeType || (webgl.VertexAttributeType = {}));
        var VertexAttributeType = webgl.VertexAttributeType;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var PolygonBatch = (function () {
            function PolygonBatch() {
            }
            return PolygonBatch;
        }());
        webgl.PolygonBatch = PolygonBatch;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Shader = (function () {
            function Shader(_vertexShader, _fragmentShader) {
                this._vertexShader = _vertexShader;
                this._fragmentShader = _fragmentShader;
                this._vs = null;
                this._fs = null;
                this._program = null;
                this._tmp2x2 = new Float32Array(2 * 2);
                this._tmp3x3 = new Float32Array(3 * 3);
                this._tmp4x4 = new Float32Array(4 * 4);
                this.compile();
            }
            Shader.prototype.program = function () { return this._program; };
            Shader.prototype.vertexShader = function () { return this._vertexShader; };
            Shader.prototype.fragmentShader = function () { return this._fragmentShader; };
            Shader.prototype.compile = function () {
                var gl = spine.webgl.gl;
                try {
                    this._vs = this.compileShader(gl.VERTEX_SHADER, this._vertexShader);
                    this._fs = this.compileShader(gl.FRAGMENT_SHADER, this._fragmentShader);
                    this._program = this.compileProgram(this._vs, this._fs);
                }
                catch (e) {
                    this.dispose();
                    throw e;
                }
            };
            Shader.prototype.compileShader = function (type, source) {
                var shader = webgl.gl.createShader(type);
                webgl.gl.shaderSource(shader, source);
                webgl.gl.compileShader(shader);
                if (!webgl.gl.getShaderParameter(shader, webgl.gl.COMPILE_STATUS)) {
                    var error = "Couldn't compile shader: " + webgl.gl.getShaderInfoLog(shader);
                    webgl.gl.deleteShader(shader);
                    throw new Error(error);
                }
                return shader;
            };
            Shader.prototype.compileProgram = function (vs, fs) {
                var program = webgl.gl.createProgram();
                webgl.gl.attachShader(program, vs);
                webgl.gl.attachShader(program, fs);
                webgl.gl.linkProgram(program);
                if (!webgl.gl.getProgramParameter(program, webgl.gl.LINK_STATUS)) {
                    var error = "Couldn't compile shader program: " + webgl.gl.getProgramInfoLog(program);
                    webgl.gl.deleteProgram(program);
                    throw new Error(error);
                }
                return program;
            };
            Shader.prototype.bind = function () {
                webgl.gl.useProgram(this._program);
            };
            Shader.prototype.unbind = function () {
                webgl.gl.useProgram(null);
            };
            Shader.prototype.setUniformi = function (uniform, value) {
                webgl.gl.uniform1i(this.getUniformLocation(uniform), value);
            };
            Shader.prototype.setUniformf = function (uniform, value) {
                webgl.gl.uniform1f(this.getUniformLocation(uniform), value);
            };
            Shader.prototype.setUniform2f = function (uniform, value, value2) {
                webgl.gl.uniform2f(this.getUniformLocation(uniform), value, value2);
            };
            Shader.prototype.setUniform3f = function (uniform, value, value2, value3) {
                webgl.gl.uniform3f(this.getUniformLocation(uniform), value, value2, value3);
            };
            Shader.prototype.setUniform4f = function (uniform, value, value2, value3, value4) {
                webgl.gl.uniform4f(this.getUniformLocation(uniform), value, value2, value3, value4);
            };
            Shader.prototype.setUniform2x2f = function (uniform, value) {
                this._tmp2x2.set(value);
                webgl.gl.uniformMatrix2fv(this.getUniformLocation(uniform), false, this._tmp2x2);
            };
            Shader.prototype.setUniform3x3f = function (uniform, value) {
                this._tmp3x3.set(value);
                webgl.gl.uniformMatrix3fv(this.getUniformLocation(uniform), false, this._tmp3x3);
            };
            Shader.prototype.setUniform4x4f = function (uniform, value) {
                this._tmp4x4.set(value);
                webgl.gl.uniformMatrix4fv(this.getUniformLocation(uniform), false, this._tmp4x4);
            };
            Shader.prototype.getUniformLocation = function (uniform) {
                var location = webgl.gl.getUniformLocation(this._program, uniform);
                if (!location)
                    throw new Error("Couldn't find location for uniform " + uniform);
                return location;
            };
            Shader.prototype.getAttributeLocation = function (attribute) {
                var location = webgl.gl.getAttribLocation(this._program, attribute);
                if (location == -1)
                    throw new Error("Couldn't find location for attribute " + attribute);
                return location;
            };
            Shader.prototype.dispose = function () {
                if (this._vs) {
                    webgl.gl.deleteShader(this._vs);
                    this._vs = null;
                }
                if (this._fs) {
                    webgl.gl.deleteShader(this._fs);
                    this._fs = null;
                }
                if (this._program) {
                    webgl.gl.deleteProgram(this._program);
                    this._program = null;
                }
            };
            Shader.newColoredTextured = function () {
                var vs = "\n                attribute vec4 " + Shader.POSITION + ";\n                attribute vec4 " + Shader.COLOR + ";\n                attribute vec2 " + Shader.TEXCOORDS + ";\n                uniform mat4 " + Shader.MVP_MATRIX + ";\n                varying vec4 v_color;\n                varying vec2 v_texCoords;\n            \n                void main() {                    \n                    v_color = " + Shader.COLOR + ";                    \n                    v_texCoords = " + Shader.TEXCOORDS + ";\n                    gl_Position =  " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n                }\n            ";
                var fs = "\n                #ifdef GL_ES\n\t\t\t        #define LOWP lowp\n\t\t\t        precision mediump float;\n\t\t\t    #else\n\t\t\t        #define LOWP \n\t\t\t    #endif\n\t\t\t    varying LOWP vec4 v_color;\n\t\t\t    varying vec2 v_texCoords;\n\t\t\t    uniform sampler2D u_texture;\n\n\t\t\t    void main() {\t\t\t    \n\t\t\t        gl_FragColor = v_color * texture2D(u_texture, v_texCoords);\n\t\t\t    }\n            ";
                return new Shader(vs, fs);
            };
            Shader.newColored = function () {
                var vs = "\n                attribute vec4 " + Shader.POSITION + ";\n                attribute vec4 " + Shader.COLOR + ";            \n                uniform mat4 " + Shader.MVP_MATRIX + ";\n                varying vec4 v_color;                \n            \n                void main() {                    \n                    v_color = " + Shader.COLOR + ";                    \n                    gl_Position =  " + Shader.MVP_MATRIX + " * " + Shader.POSITION + ";\n                }\n            ";
                var fs = "\n                #ifdef GL_ES\n\t\t\t        #define LOWP lowp\n\t\t\t        precision mediump float;\n\t\t\t    #else\n\t\t\t        #define LOWP\n\t\t\t    #endif\n\t\t\t    varying LOWP vec4 v_color;\t\t\t    \t\t\t    \n\n\t\t\t    void main() {\t\t\t    \n\t\t\t        gl_FragColor = v_color;\n\t\t\t    }\n            ";
                return new Shader(vs, fs);
            };
            Shader.MVP_MATRIX = "u_projTrans";
            Shader.POSITION = "a_position";
            Shader.COLOR = "a_color";
            Shader.TEXCOORDS = "a_texCoords";
            Shader.SAMPLER = "u_texture";
            return Shader;
        }());
        webgl.Shader = Shader;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Texture = (function () {
            function Texture(image, useMipMaps) {
                if (useMipMaps === void 0) { useMipMaps = false; }
                this._boundUnit = 0;
                this._texture = webgl.gl.createTexture();
                this._image = image;
                this.update(useMipMaps);
            }
            Texture.prototype.getImage = function () {
                return this._image;
            };
            Texture.prototype.update = function (useMipMaps) {
                this.bind();
                webgl.gl.texImage2D(webgl.gl.TEXTURE_2D, 0, webgl.gl.RGBA, webgl.gl.RGBA, webgl.gl.UNSIGNED_BYTE, this._image);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MAG_FILTER, webgl.gl.LINEAR);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_MIN_FILTER, useMipMaps ? webgl.gl.LINEAR_MIPMAP_LINEAR : webgl.gl.LINEAR);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_S, webgl.gl.CLAMP_TO_EDGE);
                webgl.gl.texParameteri(webgl.gl.TEXTURE_2D, webgl.gl.TEXTURE_WRAP_T, webgl.gl.CLAMP_TO_EDGE);
                if (useMipMaps)
                    webgl.gl.generateMipmap(webgl.gl.TEXTURE_2D);
                this.unbind();
            };
            Texture.prototype.bind = function (unit) {
                if (unit === void 0) { unit = 0; }
                this._boundUnit = unit;
                webgl.gl.activeTexture(webgl.gl.TEXTURE0 + unit);
                webgl.gl.bindTexture(webgl.gl.TEXTURE_2D, this._texture);
            };
            Texture.prototype.unbind = function () {
                webgl.gl.activeTexture(webgl.gl.TEXTURE0 + this._boundUnit);
                webgl.gl.bindTexture(webgl.gl.TEXTURE_2D, null);
            };
            Texture.prototype.dispose = function () {
                webgl.gl.deleteTexture(this._texture);
            };
            return Texture;
        }());
        webgl.Texture = Texture;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        var Vector3 = (function () {
            function Vector3() {
                this.x = 0;
                this.y = 0;
                this.z = 0;
            }
            Vector3.prototype.set = function (x, y, z) {
                this.x = x;
                this.y = y;
                this.z = z;
                return this;
            };
            Vector3.prototype.add = function (v) {
                this.x += v.x;
                this.y += v.y;
                this.z += v.z;
                return this;
            };
            Vector3.prototype.sub = function (v) {
                this.x -= v.x;
                this.y -= v.y;
                this.z -= v.z;
                return this;
            };
            Vector3.prototype.scale = function (s) {
                this.x *= s;
                this.y *= s;
                this.z *= s;
                return this;
            };
            Vector3.prototype.normalize = function () {
                var len = this.length();
                if (len == 0)
                    return this;
                len = 1 / len;
                this.x *= len;
                this.y *= len;
                this.z *= len;
                return this;
            };
            Vector3.prototype.cross = function (v) {
                return this.set(this.y * v.z - this.z * v.y, this.z * v.x - this.x * v.z, this.x * v.y - this.y * v.x);
            };
            Vector3.prototype.multiply = function (matrix) {
                var l_mat = matrix.values;
                return this.set(this.x * l_mat[webgl.M00] + this.y * l_mat[webgl.M01] + this.z * l_mat[webgl.M02] + l_mat[webgl.M03], this.x * l_mat[webgl.M10] + this.y * l_mat[webgl.M11] + this.z * l_mat[webgl.M12] + l_mat[webgl.M13], this.x * l_mat[webgl.M20] + this.y * l_mat[webgl.M21] + this.z * l_mat[webgl.M22] + l_mat[webgl.M23]);
            };
            Vector3.prototype.project = function (matrix) {
                var l_mat = matrix.values;
                var l_w = 1 / (this.x * l_mat[webgl.M30] + this.y * l_mat[webgl.M31] + this.z * l_mat[webgl.M32] + l_mat[webgl.M33]);
                return this.set((this.x * l_mat[webgl.M00] + this.y * l_mat[webgl.M01] + this.z * l_mat[webgl.M02] + l_mat[webgl.M03]) * l_w, (this.x * l_mat[webgl.M10] + this.y * l_mat[webgl.M11] + this.z * l_mat[webgl.M12] + l_mat[webgl.M13]) * l_w, (this.x * l_mat[webgl.M20] + this.y * l_mat[webgl.M21] + this.z * l_mat[webgl.M22] + l_mat[webgl.M23]) * l_w);
            };
            Vector3.prototype.dot = function (v) {
                return this.x * v.x + this.y * v.y + this.z * v.z;
            };
            Vector3.prototype.length = function () {
                return Math.sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            };
            Vector3.prototype.distance = function (v) {
                var a = v.x - this.x;
                var b = v.y - this.y;
                var c = v.z - this.z;
                return Math.sqrt(a * a + b * b + c * c);
            };
            return Vector3;
        }());
        webgl.Vector3 = Vector3;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
var spine;
(function (spine) {
    var webgl;
    (function (webgl) {
        function init(gl) {
            if (!gl || !(gl instanceof WebGLRenderingContext))
                throw Error("Expected a WebGLRenderingContext");
            spine.webgl.gl = gl;
        }
        webgl.init = init;
    })(webgl = spine.webgl || (spine.webgl = {}));
})(spine || (spine = {}));
//# sourceMappingURL=spine.js.map