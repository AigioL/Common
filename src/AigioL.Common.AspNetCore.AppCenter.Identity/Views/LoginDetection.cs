using AigioL.Common.AspNetCore.AppCenter.Identity.Models;
using AigioL.Common.AspNetCore.AppCenter.Identity.Models.Shared;
using AigioL.Common.AspNetCore.AppCenter.Identity.Views.Shared;
using System.Buffers;
using System.Text.Encodings.Web;

#pragma warning disable IDE0290 // 使用主构造函数
namespace AigioL.Common.AspNetCore.AppCenter.Identity.Views;

public sealed class LoginDetection : _Layout<LoginDetectionModel>
{
    public LoginDetection(LoginDetectionModel model, JavaScriptEncoder? encoder = null, int statusCode = DefaultStatusCode, bool useResponseCompression = DefaultUseResponseCompression) : base(model, encoder, statusCode, useResponseCompression)
    {
    }

    protected override LayoutModel GetLayoutModel() => model.Layout;

    protected override Task RenderBodyAsync(HttpContext context)
    {
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
<div id="root">
    <div class="loading">
        <svg class="circular" viewBox="25 25 50 50">
            <circle class="path" cx="50" cy="50" r="20" fill="none" stroke-width="3" stroke-miterlimit="10"></circle>
        </svg>
        <p>
"""u8);
        context.Response.BodyWriter.WriteHtmlEncodedText(model.PleaseWait);
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
</p>
    </div>
</div>
<div id="open-modal" class="modal-window">
    <div class="modal-content" id="modalContent"></div>
</div>
"""u8);
        return Task.CompletedTask;
    }

    Task RenderScriptsAsync(HttpContext context)
    {
        Modernizr210815Js(context);
        BrowserIsSupportedJs(context);
        PageJs(context);
        return Task.CompletedTask;
    }

    Task RenderStylesAsync(HttpContext context)
    {
        context.Response.BodyWriter.WriteRemoveNewLineTrim(
"""
<style type="text/css">
    body {
        background-color: transparent;
    }

    a, .clickDiv {
        color: #1890ff;
        text-decoration: none;
        background-color: #0000;
        outline: none;
        cursor: pointer;
        transition: color .3s;
    }

    .circular {
        -webkit-animation: rotate 2s linear infinite;
        animation: rotate 2s linear infinite;
        height: 100%;
        transform-origin: center center;
        width: 100%;
        position: absolute;
        top: 0;
        bottom: 0;
        left: 0;
        right: 0;
        margin: auto;
    }

    .path {
        stroke-dasharray: 1, 200;
        stroke-dashoffset: 0;
        -webkit-animation: dash 1.5s ease-in-out infinite, color 6s ease-in-out infinite;
        animation: dash 1.5s ease-in-out infinite, color 6s ease-in-out infinite;
        stroke-linecap: round;
        stroke: #0078d7;
    }

    .loading:before {
        content: "";
        display: block;
        padding-top: 100%;
    }

    @-webkit-keyframes rotate {
        100% {
            transform: rotate(360deg);
        }
    }

    @keyframes rotate {
        100% {
            transform: rotate(360deg);
        }
    }

    @-webkit-keyframes dash {
        0% {
            stroke-dasharray: 1, 200;
            stroke-dashoffset: 0;
        }

        50% {
            stroke-dasharray: 89, 200;
            stroke-dashoffset: -35px;
        }

        100% {
            stroke-dasharray: 89, 200;
            stroke-dashoffset: -124px;
        }
    }

    @keyframes dash {
        0% {
            stroke-dasharray: 1, 200;
            stroke-dashoffset: 0;
        }

        50% {
            stroke-dasharray: 89, 200;
            stroke-dashoffset: -35px;
        }

        100% {
            stroke-dasharray: 89, 200;
            stroke-dashoffset: -124px;
        }
    }

    .loading {
        position: relative;
        margin: 0 auto;
        text-align: center;
        width: 100px;
    }

    #root {
        position: fixed;
        top: 50%;
        left: 50%;
        width: 414px;
        transform: translateX(-50%) translateY(-50%);
    }


    .body-dark #root {
        color: white;
    }

    .body-dark a {
        color: #1890FF;
    }

    .transparent {
        background-color: transparent;
    }

    .modal-window {
        position: fixed;
        background-color: rgba(255, 255, 255, 0.25);
        top: 0;
        right: 0;
        bottom: 0;
        left: 0;
        z-index: 999;
        visibility: hidden;
        opacity: 0;
        pointer-events: none;
        transition: all 0.3s;
    }

        .modal-window.show {
            visibility: visible;
            opacity: 1;
            pointer-events: auto;
        }

        .modal-window > .modal-content {
            width: 434px;
            position: absolute;
            top: 50%;
            left: 50%;
            -webkit-transform: translate(-50%, -50%);
            transform: translate(-50%, -50%);
            padding: 2em;
            background: #ffffff;
            border-radius: 8px;
            border: #c1c1c159 1px solid;
        }

        .modal-window h1 {
            font-size: 150%;
            margin: 0 0 15px;
        }

    .modal-close {
        color: #aaa;
        line-height: 50px;
        font-size: 80%;
        position: absolute;
        right: 0;
        text-align: center;
        top: 0;
        width: 70px;
        text-decoration: none;
    }

        .modal-close:hover {
            color: black;
        }

    .modal-window div:not(:last-of-type) {
        margin-bottom: 15px;
    }

    .body-dark .modal-window {
        background-color: rgba(16, 16, 16, 0.25);
    }

        .body-dark .modal-window .modal-content {
            background-color: #202020;
            color: white;
        }

        .body-dark .modal-window > .modal-content {
            border-color: #181818;
        }

    .modal-content {
        box-shadow: 0 3px 6px -4px #0000001f, 0 6px 16px #00000014, 0 9px 28px 8px #0000000d;
    }

    .hide {
        display: none;
    }

    .loading p {
        padding-top: 24px;
        font-size: 22px;
    }

    .copyBody {
        word-wrap: break-word;
        padding: 5px;
        max-height: 500px;
        overflow-y: scroll;
        background-color: rgb(53,53,53,0.18);
    }

    .body-dark .copyBody {
        background-color: rgb(53,53,53,0.5);
    }

    @media (max-width:450px) {
        .modal-window > .modal-content {
            width: 90%;
        }
    }

    /*---滚动条默认显示样式--*/

    ::-webkit-scrollbar-thumb {
        background-color: #858585;
        height: 50px;
        outline-offset: -2px;
        border: 4px solid #858585;
        border-radius: 5px;
    }
    /*---滚动条大小--*/

    ::-webkit-scrollbar {
        width: 4px;
        height: 4px;
    }

    /*---滚动框背景样式--*/

    ::-webkit-scrollbar-track-piece {
        background-color: #1D1D1D;
        -webkit-border-radius: 0;
    }
</style>
"""u8);
        return Task.CompletedTask;
    }

    void Modernizr210815Js(HttpContext context)
    {
        context.Response.BodyWriter.WriteAllLineTrim(
"""
<script type="text/javascript">
/*! modernizr 3.6.0 (Custom Build) | MIT *
 * https://modernizr.com/download/?-boxshadow-cssanimations-csstransforms-json-svg-templatestrings-websockets-setclasses !*/
!function(window,document,undefined){function setClasses(e){var t=docElement.className,n=Modernizr._config.classPrefix||"";if(isSVG&&(t=t.baseVal),Modernizr._config.enableJSClass){var r=new RegExp("(^|\\s)"+n+"no-js(\\s|$)");t=t.replace(r,"$1"+n+"js$2")}Modernizr._config.enableClasses&&(t+=" "+n+e.join(" "+n),isSVG?docElement.className.baseVal=t:docElement.className=t)}function is(e,t){return typeof e===t}function testRunner(){var e,t,n,r,o,s,i;for(var l in tests)if(tests.hasOwnProperty(l)){if(e=[],t=tests[l],t.name&&(e.push(t.name.toLowerCase()),t.options&&t.options.aliases&&t.options.aliases.length))for(n=0;n<t.options.aliases.length;n++)e.push(t.options.aliases[n].toLowerCase());for(r=is(t.fn,"function")?t.fn():t.fn,o=0;o<e.length;o++)s=e[o],i=s.split("."),1===i.length?Modernizr[i[0]]=r:(!Modernizr[i[0]]||Modernizr[i[0]]instanceof Boolean||(Modernizr[i[0]]=new Boolean(Modernizr[i[0]])),Modernizr[i[0]][i[1]]=r),classes.push((r?"":"no-")+i.join("-"))}}function contains(e,t){return!!~(""+e).indexOf(t)}function createElement(){return"function"!=typeof document.createElement?document.createElement(arguments[0]):isSVG?document.createElementNS.call(document,"http://www.w3.org/2000/svg",arguments[0]):document.createElement.apply(document,arguments)}function cssToDOM(e){return e.replace(/([a-z])-([a-z])/g,function(e,t,n){return t+n.toUpperCase()}).replace(/^-/,"")}function fnBind(e,t){return function(){return e.apply(t,arguments)}}function testDOMProps(e,t,n){var r;for(var o in e)if(e[o]in t)return n===!1?e[o]:(r=t[e[o]],is(r,"function")?fnBind(r,n||t):r);return!1}function domToCSS(e){return e.replace(/([A-Z])/g,function(e,t){return"-"+t.toLowerCase()}).replace(/^ms-/,"-ms-")}function computedStyle(e,t,n){var r;if("getComputedStyle"in window){r=getComputedStyle.call(window,e,t);var o=window.console;if(null!==r)n&&(r=r.getPropertyValue(n));else if(o){var s=o.error?"error":"log";o[s].call(o,"getComputedStyle returning null, its possible modernizr test results are inaccurate")}}else r=!t&&e.currentStyle&&e.currentStyle[n];return r}function getBody(){var e=document.body;return e||(e=createElement(isSVG?"svg":"body"),e.fake=!0),e}function injectElementWithStyles(e,t,n,r){var o,s,i,l,d="modernizr",a=createElement("div"),c=getBody();if(parseInt(n,10))for(;n--;)i=createElement("div"),i.id=r?r[n]:d+(n+1),a.appendChild(i);return o=createElement("style"),o.type="text/css",o.id="s"+d,(c.fake?c:a).appendChild(o),c.appendChild(a),o.styleSheet?o.styleSheet.cssText=e:o.appendChild(document.createTextNode(e)),a.id=d,c.fake&&(c.style.background="",c.style.overflow="hidden",l=docElement.style.overflow,docElement.style.overflow="hidden",docElement.appendChild(c)),s=t(a,e),c.fake?(c.parentNode.removeChild(c),docElement.style.overflow=l,docElement.offsetHeight):a.parentNode.removeChild(a),!!s}function nativeTestProps(e,t){var n=e.length;if("CSS"in window&&"supports"in window.CSS){for(;n--;)if(window.CSS.supports(domToCSS(e[n]),t))return!0;return!1}if("CSSSupportsRule"in window){for(var r=[];n--;)r.push("("+domToCSS(e[n])+":"+t+")");return r=r.join(" or "),injectElementWithStyles("@supports ("+r+") { #modernizr { position: absolute; } }",function(e){return"absolute"==computedStyle(e,null,"position")})}return undefined}function testProps(e,t,n,r){function o(){i&&(delete mStyle.style,delete mStyle.modElem)}if(r=is(r,"undefined")?!1:r,!is(n,"undefined")){var s=nativeTestProps(e,n);if(!is(s,"undefined"))return s}for(var i,l,d,a,c,u=["modernizr","tspan","samp"];!mStyle.style&&u.length;)i=!0,mStyle.modElem=createElement(u.shift()),mStyle.style=mStyle.modElem.style;for(d=e.length,l=0;d>l;l++)if(a=e[l],c=mStyle.style[a],contains(a,"-")&&(a=cssToDOM(a)),mStyle.style[a]!==undefined){if(r||is(n,"undefined"))return o(),"pfx"==t?a:!0;try{mStyle.style[a]=n}catch(f){}if(mStyle.style[a]!=c)return o(),"pfx"==t?a:!0}return o(),!1}function testPropsAll(e,t,n,r,o){var s=e.charAt(0).toUpperCase()+e.slice(1),i=(e+" "+cssomPrefixes.join(s+" ")+s).split(" ");return is(t,"string")||is(t,"undefined")?testProps(i,t,r,o):(i=(e+" "+domPrefixes.join(s+" ")+s).split(" "),testDOMProps(i,t,n))}function testAllProps(e,t,n){return testPropsAll(e,undefined,undefined,t,n)}var classes=[],tests=[],ModernizrProto={_version:"3.6.0",_config:{classPrefix:"",enableClasses:!0,enableJSClass:!0,usePrefixes:!0},_q:[],on:function(e,t){var n=this;setTimeout(function(){t(n[e])},0)},addTest:function(e,t,n){tests.push({name:e,fn:t,options:n})},addAsyncTest:function(e){tests.push({name:null,fn:e})}},Modernizr=function(){};Modernizr.prototype=ModernizrProto,Modernizr=new Modernizr,Modernizr.addTest("json","JSON"in window&&"parse"in JSON&&"stringify"in JSON),Modernizr.addTest("svg",!!document.createElementNS&&!!document.createElementNS("http://www.w3.org/2000/svg","svg").createSVGRect);var supports=!1;try{supports="WebSocket"in window&&2===window.WebSocket.CLOSING}catch(e){}Modernizr.addTest("websockets",supports);var docElement=document.documentElement,isSVG="svg"===docElement.nodeName.toLowerCase(),omPrefixes="Moz O ms Webkit",cssomPrefixes=ModernizrProto._config.usePrefixes?omPrefixes.split(" "):[];ModernizrProto._cssomPrefixes=cssomPrefixes;var domPrefixes=ModernizrProto._config.usePrefixes?omPrefixes.toLowerCase().split(" "):[];ModernizrProto._domPrefixes=domPrefixes;var modElem={elem:createElement("modernizr")};Modernizr._q.push(function(){delete modElem.elem});var mStyle={style:modElem.elem.style};Modernizr._q.unshift(function(){delete mStyle.style}),ModernizrProto.testAllProps=testPropsAll,ModernizrProto.testAllProps=testAllProps,Modernizr.addTest("cssanimations",testAllProps("animationName","a",!0)),Modernizr.addTest("boxshadow",testAllProps("boxShadow","1px 1px",!0)),Modernizr.addTest("csstransforms",function(){return-1===navigator.userAgent.indexOf("Android 2.")&&testAllProps("transform","scale(1)",!0)}),Modernizr.addTest("templatestrings",function(){var supports;try{eval("``"),supports=!0}catch(e){}return!!supports}),testRunner(),setClasses(classes),delete ModernizrProto.addTest,delete ModernizrProto.addAsyncTest;for(var i=0;i<Modernizr._q.length;i++)Modernizr._q[i]();window.Modernizr=Modernizr}(window,document);
</script>
"""u8);
    }

    void BrowserIsSupportedJs(HttpContext context)
    {
        context.Response.BodyWriter.WriteAllLineTrim(
"""
<script type="text/javascript">
    (function () {
        var err = '';
        if (function () {
            try {
                for (var e in Modernizr)
                    if ("boolean" == typeof Modernizr[e] && !Modernizr[e])
                        return 1
            } catch (ex) {
                err = ex.stack;
                return 1;
            }
        }()) {
            var e = document.createElement("div");
            if (err.length > 0) {
                e.appendChild(document.createTextNode("Error:"));
                e.appendChild(document.createTextNode(err));
            } else {
                e.appendChild(document.createTextNode("
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.BrowserIsSupported1, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
) "));
                e.style.fontSize = "22px";
                e.setAttribute("id", "low-ver-browser");
                var t = document.createElement("a");
                t.appendChild(document.createTextNode("Microsoft Edge"));
                t.setAttribute("href", "
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Url_MicrosoftEdge, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
");
                t.setAttribute("target", "_blank");
                e.appendChild(t);
                e.appendChild(document.createTextNode("，"));
                (t = document.createElement("a"))
                    .appendChild(document.createTextNode("Google Chrome"));
                t.setAttribute("href", "
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Url_Chrome, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
");
                t.setAttribute("target", "_blank");
                e.appendChild(t);
                e.appendChild(document.createTextNode("，"));
                (t = document.createElement("a"))
                    .appendChild(document.createTextNode("Mozilla Firefox"));
                t.setAttribute("href", "
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Url_Firefox, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
");
                t.setAttribute("target", "_blank");
                e.appendChild(t);
                e.appendChild(document.createTextNode(" 
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.BrowserIsSupported2, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
"));
            }
            document.body.innerHTML = "";
            document.body.appendChild(e);
            document.body.style.backgroundColor = "#ffffff";
            document.body.parentNode.style.backgroundColor = "#ffffff";
            document.body.style.padding = "22px";
        }
    })();
</script>
"""u8);
    }

    void PageJs(HttpContext context)
    {
        var u8ModelErrorTitle = model.ModelErrorTitle.GetJsonEncodedUtf8Bytes(encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
<script type="text/javascript">
    let isOk = false;
    const error = '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Error, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
    const channel = '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Channel, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
    const channelInt32 = '
"""u8);
        context.Response.BodyWriter.Write(model.ChannelInt32);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
    const newLine = '\r\n';
    var token = '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Token, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
    var isUS = '
"""u8);
        context.Response.BodyWriter.Write(model.UseUrlSchemeLoginToken, true);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
    const msgBox = document.getElementById('modalContent');
    window.onload = function () {
        function changeThemes(dark) {
            let windowEl = document.getElementsByTagName('html')[0];
            let body = document.body;
            let bodyT = window.location.hash.length == 0;
            if (dark) {
                if (body)
                    body.className = `body-dark ${bodyT ? '' : 'transparent'}`;
                if (windowEl)
                    windowEl.className = `body-dark ${bodyT ? '' : 'transparent'}`;
            } else {
                if (body)
                    body.className = bodyT ? '' : "transparent";
                if (windowEl)
                    windowEl.className = bodyT ? '' : "transparent";
            }
            if (bodyT) {
                body.style.backgroundColor = dark ? '#272727' : '#F9F9F9';
            }
        }
        try {
            let scheme = window.matchMedia('(prefers-color-scheme: dark)');
            changeThemes(scheme.matches);
            scheme.onchange = changeThemes;
        } catch { }
        if (error.length == 0) {
            if (token.length > 0) {
                if (isUS === "true") {
                    urlScheme();
                } else {
                    connect();
                }
                setTimeout(function () {
                    if (token.length > 0 && !isOk)
                        showModel2(5);
                }, 3000)
            } else {
                toLogin();
            }
        } else {
            showModel(true, `<h1>
"""u8);
        context.Response.BodyWriter.Write(u8ModelErrorTitle);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>${error}</div>`);
        }
    };
    function urlScheme() {
        const uslt = "
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.UrlSchemeLoginToken, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
";
        if (uslt.length === 0) return;
        location.href = uslt;
        setTimeout(function () {
            if (token.length > 0 && !isOk) {
                isOk = true;
                showModel2(6);
            }
        }, 3000)
    }
    function connect() {
        const port = '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.Port, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
        if (port == '' || token == '') return;
        var ws = new WebSocket("ws://127.0.0.1:" + port);
        ws.onopen = function () {
            ws.send(token);
        }
        ws.onmessage = function (e) {
            let data = JSON.parse(e.data);
            if (data.State) {
                isOk = true;
                showModel2(1);
                setTimeout(function () {
                    window.opener = null; window.open('', '_self'); window.close()
                }, 10000)
            } else {
                showModel2(2, data.Msg);
            }
        }
        ws.onclose = function (e) {
            if (!isOk) {
                showModel2(3);
            }
        }
        ws.onerror = function (e) {
            console.log(e)
            if (token.length > 0 && !isOk)
                showModel2(5);
        }
    }
    function timeOut() {
        showModel2(4);
    }
    function showModel(show, html) {
        document.getElementById('open-modal').className = `modal-window  ${show ? 'show' : ''}`;
        document.getElementById('root').className = show ? 'hide' : '';
        if (!html) html = '';
        else html = html.replaceAll(newLine, '<br/>');
        msgBox.innerHTML = html;
    }
    function toLogin() {
        if (channel == '') return;
        showModel(false);
        let loginUrl = `
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.LoginUrl, encoder);

        context.Response.BodyWriter.WriteAllLineTrim(
"""
`;
        location.href = loginUrl;
        setTimeout(timeOut, 30000);
    }
    function copyToken() {
        if (token == '') return;
        var textArea = document.createElement("textarea");
        textArea.style.position = 'fixed';
        textArea.style.top = '0';
        textArea.style.left = '0';
        textArea.style.padding = '0';
        textArea.style.border = 'none';
        textArea.style.outline = 'none';
        textArea.style.boxShadow = 'none';
        textArea.style.background = 'transparent';
        textArea.value = token;
        document.body.appendChild(textArea);
        textArea.select();
        try {
            var successful = document.execCommand('copy');
            var msg = successful ? '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.CopySuccess, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
' : '
"""u8);
        var u8CopyNoSupport = model.CopyNoSupport.GetJsonEncodedUtf8Bytes(encoder);
        context.Response.BodyWriter.Write(u8CopyNoSupport);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';
            document.getElementById('copyButton')
                .innerText = msg;

        } catch (err) {
            document.getElementById('copyButton')
                .innerText = '
"""u8);
        context.Response.BodyWriter.Write(u8CopyNoSupport);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
';

        }
        document.body.removeChild(textArea);
    }
    function showModel2(type, msg) {
        let html = '';
        switch (type) {
            case 1:
            case 6:
                html = `<h1>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.ModelSuccessTitle, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.LoginSuccessTip1, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div><div>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.LoginSuccessTip2, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div>
"""u8);
        var u8ClickHereTryAgain = model.ClickHereTryAgain.GetJsonEncodedUtf8Bytes(encoder);
        if (model.UseUrlSchemeLoginToken)
        {
            context.Response.BodyWriter.Write("<div class='clickDiv' onclick='urlScheme()'>"u8);
            context.Response.BodyWriter.Write(u8ClickHereTryAgain);
            context.Response.BodyWriter.Write("</div>"u8);
        }
        context.Response.BodyWriter.WriteAllLineTrim(
"""
`;
                break;
            case 2:
                if (!msg) msg = '';
                html = `<h1>
"""u8);
        context.Response.BodyWriter.Write(u8ModelErrorTitle);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>${msg}</div>`;
                break;
            case 3:
                html = `<h1>
"""u8);
        context.Response.BodyWriter.Write(u8ModelErrorTitle);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.WebSocketLostTip, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div><div onclick='connect()' class='clickDiv'>
"""u8);
        context.Response.BodyWriter.Write(u8ClickHereTryAgain);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div>`;
                break;
            case 4:
                var tips = '';
                if (channel === 'Steam')
                    tips = '
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.PleaseCheckSteamCommunity, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
'
                html = `<h1>
"""u8);
        context.Response.BodyWriter.Write(u8ModelErrorTitle);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.LongTimeNoJump, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
${tips}</div><div class='clickDiv' onclick='toLogin()'>
"""u8);

        context.Response.BodyWriter.Write(u8ClickHereTryAgain);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div>`;
                break;
            case 5:
                var tips = `<div class='copyBody'>${token}</div>`;
                html = `<h1>
"""u8);
        context.Response.BodyWriter.Write(u8ModelErrorTitle);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</h1><div>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.ManualCopyTip, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div>${tips}<div class='clickDiv' id='copyButton' onclick='copyToken()'>
"""u8);
        context.Response.BodyWriter.WriteJsonEncodedText(model.CopyButton, encoder);
        context.Response.BodyWriter.WriteAllLineTrim(
"""
</div>`;
                break;
        }
        if (html !== '') {
            showModel(true, html);
        } else {
            showModel(false);
        }
    }
</script>
"""u8);
    }

    protected override Task RenderSectionAsync(HttpContext context, string sectionName) => sectionName switch
    {
        Styles => RenderStylesAsync(context),
        Scripts => RenderScriptsAsync(context),
        _ => Task.CompletedTask,
    };
}
