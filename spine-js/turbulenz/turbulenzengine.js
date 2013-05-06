// Copyright (c) 2011-2012 Turbulenz Limited
/*global VMath*/
/*global WebGLGraphicsDevice*/
/*global WebGLInputDevice*/
/*global WebGLSoundDevice*/
/*global WebGLPhysicsDevice*/
/*global WebGLNetworkDevice*/
/*global Float32Array*/
/*global console*/
/*global window*/
"use strict";

//
// WebGLTurbulenzEngine
//
function WebGLTurbulenzEngine() {}
WebGLTurbulenzEngine.prototype = {

    version : '0.24.0.0',

    setInterval: function (f, t)
    {
        var that = this;
        return window.setInterval(function () {
                that.updateTime();
                f();
            }, t);
    },

    clearInterval: function (i)
    {
        return window.clearInterval(i);
    },

    createGraphicsDevice: function (params)
    {
        if (this.graphicsDevice)
        {
            this.callOnError('GraphicsDevice already created');
            return null;
        }
        else
        {
            var graphicsDevice = WebGLGraphicsDevice.create(this.canvas, params);
            this.graphicsDevice = graphicsDevice;
            return graphicsDevice;
        }
    },

    createPhysicsDevice: function (params)
    {
        if (this.physicsDevice)
        {
            this.callOnError('PhysicsDevice already created');
            return null;
        }
        else
        {
            var physicsDevice;
            var plugin = this.getPluginObject();
            if (plugin)
            {
                physicsDevice = plugin.createPhysicsDevice(params);
            }
            else
            {
                physicsDevice = WebGLPhysicsDevice.create(params);
            }
            this.physicsDevice = physicsDevice;
            return physicsDevice;
        }
    },

    createSoundDevice: function (params)
    {
        if (this.soundDevice)
        {
            this.callOnError('SoundDevice already created');
            return null;
        }
        else
        {
            var soundDevice;
            var plugin = this.getPluginObject();
            if (plugin)
            {
                soundDevice = plugin.createSoundDevice(params);
            }
            else
            {
                soundDevice = WebGLSoundDevice.create(params);
            }
            this.soundDevice = soundDevice;
            return soundDevice;
        }
    },

    createInputDevice: function (params)
    {
        if (this.inputDevice)
        {
            this.callOnError('InputDevice already created');
            return null;
        }
        else
        {
            var inputDevice = WebGLInputDevice.create(this.canvas, params);
            this.inputDevice = inputDevice;
            return inputDevice;
        }
    },

    createNetworkDevice: function (params)
    {
        if (this.networkDevice)
        {
            throw 'NetworkDevice already created';
        }
        else
        {
            var networkDevice = WebGLNetworkDevice.create(params);
            this.networkDevice = networkDevice;
            return networkDevice;
        }
    },

    createMathDevice: function (/* params */)
    {
        // Check if the browser supports using apply with Float32Array
        try
        {
            var testVector = new Float32Array([1, 2, 3]);

            VMath.v3Build.apply(VMath, testVector);

            // Clamp FLOAT_MAX
            testVector[0] = VMath.FLOAT_MAX;
            VMath.FLOAT_MAX = testVector[0];
        }
        catch (e)
        {
        }

        return VMath;
    },

    createNativeMathDevice: function (/* params */)
    {
        return VMath;
    },

    getGraphicsDevice: function ()
    {
        var graphicsDevice = this.graphicsDevice;
        if (graphicsDevice === null)
        {
            this.callOnError("GraphicsDevice not created yet.");
        }
        return graphicsDevice;
    },

    getPhysicsDevice: function ()
    {
        return this.physicsDevice;
    },

    getSoundDevice: function ()
    {
        return this.soundDevice;
    },

    getInputDevice: function ()
    {
        return this.inputDevice;
    },

    getNetworkDevice: function ()
    {
        return this.networkDevice;
    },

    getMathDevice: function ()
    {
        return VMath;
    },

    getNativeMathDevice: function ()
    {
        return VMath;
    },

    flush: function ()
    {

    },

    run: function ()
    {

    },

    encrypt: function (msg)
    {
        return msg;
    },

    decrypt: function (msg)
    {
        return msg;
    },

    generateSignature: function (/* msg */)
    {
        return null;
    },

    verifySignature: function (/* msg, sig */)
    {
        return true;
    },

    onerror: function (msg)
    {
        console.error(msg);
    },

    onwarning: function (msg)
    {
        console.warn(msg);
    },

    getSystemInfo: function ()
    {
        return this.systemInfo;
    },

    request: function (url, callback)
    {
        var that = this;

        var xhr;
        if (window.XMLHttpRequest)
        {
            xhr = new window.XMLHttpRequest();
        }
        else if (window.ActiveXObject)
        {
            xhr = new window.ActiveXObject("Microsoft.XMLHTTP");
        }
        else
        {
            that.callOnError("No XMLHTTPRequest object could be created");
            return;
        }

        function httpRequestCallback()
        {
            if (xhr.readyState === 4) /* 4 == complete */
            {
                if (!that.isUnloading())
                {
                    var xhrResponseText = xhr.responseText;
                    var xhrStatus = xhr.status;

                    if ("" === xhrResponseText)
                    {
                        xhrResponseText = null;
                    }

                    if (null === xhr.getResponseHeader("Content-Type") &&
                        "" === xhr.getAllResponseHeaders())
                    {
                        // Sometimes the browser sets status to 200 OK
                        // when the connection is closed before the
                        // message is sent (weird!).  In order to address
                        // this we fail any completely empty responses.
                        // Hopefully, nobody will get a valid response
                        // with no headers and no body!
                        // Except that for cross domain requests getAllResponseHeaders ALWAYS returns an empty string
                        // even for valid responses...
                        callback(null, 0);
                        return;
                    }

                    // Fix for loading from file
                    if (xhrStatus === 0 && xhrResponseText && window.location.protocol === "file:")
                    {
                        xhrStatus = 200;
                    }

                    // Invoke the callback
                    if (xhrStatus !== 0)
                    {
                        // Under these conditions, we return a null
                        // response text.

                        if (404 === xhrStatus)
                        {
                            xhrResponseText = null;
                        }

                        callback(xhrResponseText, xhrStatus);
                    }
                    else
                    {
                        // Checking xhr.statusText when xhr.status is
                        // 0 causes a silent error

                        callback(xhrResponseText, 0);
                    }
                }

                // break circular reference
                xhr.onreadystatechange = null;
                xhr = null;
                callback = null;
            }
        }

        xhr.open('GET', url, true);
        if (callback)
        {
            xhr.onreadystatechange = httpRequestCallback;
        }
        xhr.send();
    },

    // Internals
    destroy : function ()
    {
        if (this.networkDevice)
        {
            delete this.networkDevice;
        }
        if (this.inputDevice)
        {
            this.inputDevice.destroy();
            delete this.inputDevice;
        }
        if (this.physicsDevice)
        {
            delete this.physicsDevice;
        }
        if (this.soundDevice)
        {
            if (this.soundDevice.destroy)
            {
                this.soundDevice.destroy();
            }
            delete this.soundDevice;
        }
        if (this.graphicsDevice)
        {
            this.graphicsDevice.destroy();
            delete this.graphicsDevice;
        }
        if (this.canvas)
        {
            delete this.canvas;
        }
        if (this.resizeCanvas)
        {
            window.removeEventListener('resize', this.resizeCanvas, false);
        }
    },

    getPluginObject : function ()
    {
        if (!this.plugin &&
            this.pluginId)
        {
            this.plugin = document.getElementById(this.pluginId);
        }
        return this.plugin;
    },

    unload : function ()
    {
        if (!this.unloading)
        {
            this.unloading = true;
            if (this.onunload)
            {
                this.onunload();
            }
            if (this.destroy)
            {
                this.destroy();
            }
        }
    },

    isUnloading : function ()
    {
        return this.unloading;
    },

    enableProfiling : function ()
    {
    },

    startProfiling : function ()
    {
        if (console && console.profile && console.profileEnd)
        {
            console.profile("turbulenz");
        }
    },

    stopProfiling : function ()
    {
        // Chrome and Safari return an object. IE and Firefox print to the console/profile tab.
        var result;
        if (console && console.profile && console.profileEnd)
        {
            console.profileEnd("turbulenz");
            if (console.profiles)
            {
                result = console.profiles[console.profiles.length - 1];
            }
        }

        return result;
    },

    callOnError : function (msg)
    {
        var onerror = this.onerror;
        if (onerror)
        {
            onerror(msg);
        }
    }
};

// Constructor function
WebGLTurbulenzEngine.create = function webGLTurbulenzEngineFn(params)
{
    var tz = new WebGLTurbulenzEngine();

    var canvas = params.canvas;
    var fillParent = params.fillParent;

    // To expose unload (the whole interaction needs a re-design)
    window.TurbulenzEngineCanvas = tz;

    tz.pluginId = params.pluginId;
    tz.plugin = null;

    // time property
    var getTime = Date.now;
    var performance = window.performance;
    if (performance)
    {
        // It seems high resolution "now" requires a proper "this"
        if (performance.now)
        {
            getTime = function getTimeFn()
            {
                return performance.now();
            };
        }
        else if (performance.webkitNow)
        {
            getTime = function getTimeFn()
            {
                return performance.webkitNow();
            };
        }
    }

    // To be used by the GraphicsDevice for accurate fps calculations
    tz.getTime = getTime;

    var baseTime = getTime(); // all in milliseconds (our "time" property is in seconds)

    // Safari 6.0 has broken object property defines.
    var canUseDefineProperty = true;
    var navStr = navigator.userAgent;
    var navVersionIdx = navStr.indexOf("Version/6.0");
    if (-1 !== navVersionIdx)
    {
        if (-1 !== navStr.substring(navVersionIdx).indexOf("Safari/"))
        {
            canUseDefineProperty = false;
        }
    }

    if (canUseDefineProperty && Object.defineProperty)
    {
        Object.defineProperty(tz, "time", {
                get : function () {
                    return ((getTime() - baseTime) * 0.001);
                },
                set : function (newValue) {
                    if (typeof newValue === 'number')
                    {
                        // baseTime is in milliseconds, newValue is in seconds
                        baseTime = (getTime() - (newValue * 1000));
                    }
                    else
                    {
                        tz.callOnError("Must set 'time' attribute to a number");
                    }
                },
                enumerable : false,
                configurable : false
            });

        tz.updateTime = function ()
        {
        };
    }
    else
    {
        tz.updateTime = function ()
        {
            this.time = ((getTime() - baseTime) * 0.001);
        };
    }

    // fast zero timeouts
    if (window.postMessage)
    {
        var zeroTimeoutMessageName = "0-timeout-message";
        var timeouts = [];
        var timeId = 0;

        var setZeroTimeout = function setZeroTimeoutFn(fn)
        {
            timeId += 1;
            var timeout = {
                    id : timeId,
                    fn : fn
                };
            timeouts.push(timeout);
            window.postMessage(zeroTimeoutMessageName, "*");
            return timeout;
        };

        var clearZeroTimeout = function clearZeroTimeoutFn(timeout)
        {
            var id = timeout;
            var numTimeouts = timeouts.length;
            for (var n = 0; n < numTimeouts; n += 1)
            {
                if (timeouts[n].id === id)
                {
                    timeouts.splice(n, 1);
                    return;
                }
            }
        };

        var handleZeroTimeoutMessages = function handleZeroTimeoutMessagesFn(event)
        {
            if (event.source === window &&
                event.data === zeroTimeoutMessageName)
            {
                event.stopPropagation();

                if (timeouts.length && !tz.isUnloading())
                {
                    var timeout = timeouts.shift();
                    var fn = timeout.fn;
                    fn();
                }
            }
        };
        window.addEventListener("message", handleZeroTimeoutMessages, true);

        tz.setTimeout = function (f, t)
        {
            if (t < 1)
            {
                return setZeroTimeout(f);
            }
            else
            {
                var that = this;
                return window.setTimeout(function () {
                        that.updateTime();
                        if (!that.isUnloading())
                        {
                            f();
                        }
                    }, t);
            }
        };

        tz.clearTimeout = function (i)
        {
            if (typeof i === 'object')
            {
                return clearZeroTimeout(i);
            }
            else
            {
                return window.clearTimeout(i);
            }
        };
    }
    else
    {
        tz.setTimeout = function (f, t)
        {
            var that = this;
            return window.setTimeout(function () {
                    that.updateTime();
                    if (!that.isUnloading())
                    {
                        f();
                    }
                }, t);
        };

        tz.clearTimeout = function (i)
        {
            return window.clearTimeout(i);
        };
    }

    var requestAnimationFrame = (window.requestAnimationFrame       ||
                                 window.webkitRequestAnimationFrame ||
                                 window.oRequestAnimationFrame      ||
                                 window.msRequestAnimationFrame     ||
                                 window.mozRequestAnimationFrame);
    if (requestAnimationFrame)
    {
        tz.setInterval = function (f, t)
        {
            var that = this;
            if (Math.abs(t - (1000 / 60)) <= 1)
            {
                var interval = {
                    enabled: true
                };
                var wrap1 = function wrap1()
                {
                    if (interval.enabled)
                    {
                        that.updateTime();
                        if (!that.isUnloading())
                        {
                            f();
                        }
                        requestAnimationFrame(wrap1, that.canvas);
                    }
                };
                requestAnimationFrame(wrap1, that.canvas);
                return interval;
            }
            else
            {
                var wrap2 = function wrap2()
                {
                    that.updateTime();
                    if (!that.isUnloading())
                    {
                        f();
                    }
                };
                return window.setInterval(wrap2, t);
            }
        };

        tz.clearInterval = function (i)
        {
            if (typeof i === 'object')
            {
                i.enabled = false;
            }
            else
            {
                window.clearInterval(i);
            }
        };
    }

    tz.canvas = canvas;
    tz.networkDevice = null;
    tz.inputDevice = null;
    tz.physicsDevice = null;
    tz.soundDevice = null;
    tz.graphicsDevice = null;

    if (fillParent)
    {
        // Resize canvas to fill parent
        tz.resizeCanvas = function ()
        {
            canvas.width = canvas.parentNode.clientWidth;
            canvas.height = canvas.parentNode.clientHeight;
        };

        tz.resizeCanvas();

        window.addEventListener('resize', tz.resizeCanvas, false);
    }

    var previousOnBeforeUnload = window.onbeforeunload;
    window.onbeforeunload = function ()
    {
        tz.unload();

        if (previousOnBeforeUnload)
        {
            previousOnBeforeUnload.call(this);
        }
    };

    tz.time = 0;

    // System info
    var systemInfo = {
        architecture: '',
        cpuDescription: '',
        cpuVendor: '',
        numPhysicalCores: 1,
        numLogicalCores: 1,
        ramInMegabytes: 0,
        frequencyInMegaHZ: 0,
        osVersionMajor: 0,
        osVersionMinor: 0,
        osVersionBuild: 0,
        osName: navigator.platform,
        userLocale: (navigator.language || navigator.userLanguage).replace('-', '_')
    };
    var userAgent = navigator.userAgent;
    var osIndex = userAgent.indexOf('Windows');
    if (osIndex !== -1)
    {
        systemInfo.osName = 'Windows';
        if (navigator.platform === 'Win64')
        {
            systemInfo.architecture = 'x86_64';
        }
        else if (navigator.platform === 'Win32')
        {
            systemInfo.architecture = 'x86';
        }
        osIndex += 7;
        if (userAgent.slice(osIndex, (osIndex + 4)) === ' NT ')
        {
            osIndex += 4;
            systemInfo.osVersionMajor = parseInt(userAgent.slice(osIndex, (osIndex + 1)), 10);
            systemInfo.osVersionMinor = parseInt(userAgent.slice((osIndex + 2), (osIndex + 4)), 10);
        }
    }
    else
    {
        osIndex = userAgent.indexOf('Mac OS X');
        if (osIndex !== -1)
        {
            systemInfo.osName = 'Darwin';
            if (navigator.platform.indexOf('Intel') !== -1)
            {
                systemInfo.architecture = 'x86';
            }
            osIndex += 9;
            systemInfo.osVersionMajor = parseInt(userAgent.slice(osIndex, (osIndex + 2)), 10);
            systemInfo.osVersionMinor = parseInt(userAgent.slice((osIndex + 3), (osIndex + 4)), 10);
            systemInfo.osVersionBuild = (parseInt(userAgent.slice((osIndex + 5), (osIndex + 6)), 10) || 0);
        }
        else
        {
            osIndex = userAgent.indexOf('Linux');
            if (osIndex !== -1)
            {
                systemInfo.osName = 'Linux';
                if (navigator.platform.indexOf('64') !== -1)
                {
                    systemInfo.architecture = 'x86_64';
                }
                else if (navigator.platform.indexOf('x86') !== -1)
                {
                    systemInfo.architecture = 'x86';
                }
            }
        }
    }
    tz.systemInfo = systemInfo;

    var b64ConversionTable = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=".split('');

    tz.base64Encode = function base64EncodeFn(bytes)
    {
        var output = "";
        var numBytes = bytes.length;
        var valueToChar = b64ConversionTable;
        var n, chr1, chr2, chr3, enc1, enc2, enc3, enc4;

        /*jshint bitwise: false*/
        n = 0;
        while (n < numBytes)
        {
            chr1 = bytes[n];
            n += 1;

            enc1 = (chr1 >> 2);

            if (n < numBytes)
            {
                chr2 = bytes[n];
                n += 1;

                if (n < numBytes)
                {
                    chr3 = bytes[n];
                    n += 1;

                    enc2 = (((chr1 & 3) << 4) | (chr2 >> 4));
                    enc3 = (((chr2 & 15) << 2) | (chr3 >> 6));
                    enc4 = (chr3 & 63);
                }
                else
                {
                    enc2 = (((chr1 & 3) << 4) | (chr2 >> 4));
                    enc3 = ((chr2 & 15) << 2);
                    enc4 = 64;
                }
            }
            else
            {
                enc2 = ((chr1 & 3) << 4);
                enc3 = 64;
                enc4 = 64;
            }

            output += valueToChar[enc1];
            output += valueToChar[enc2];
            output += valueToChar[enc3];
            output += valueToChar[enc4];
        }
        /*jshint bitwise: true*/

        return output;
    };

    return tz;
};

window.WebGLTurbulenzEngine = WebGLTurbulenzEngine;
