var sky = sky || {};

// handy sky extensions to make swagger nicer links and improved api key support
sky.extensions = {
  setApiKey: function () {
    var apiParam = document.location.search.match(/api_key=([^&]*)/);
    var apiKey = apiParam && apiParam.length > 1 ? apiParam[1] : "put key here";
    $('#input_apiKey').val(apiKey);
  },

  addAnchors: function ($root) {
    var isXml = $root.hasClass("xml");
    var nodes = [];
    if (isXml) {
      nodes = $root.find("code").find(":not(iframe)").addBack().contents().filter(function () {
                return this.nodeType == 3  // text nodes
                    && $(this).prev().find(".title:contains(href)").length > 0 // inside href nodes
                    && this.textContent.replace(/[ \n]/g, '').length > 0;
                  });

    } else {
      nodes = $root.find("code").find("span.string")
      .filter(function () { return $(this).parent().prev().text() == "href"; })
    }
    var apiParam = "api_key=" + $('#input_apiKey').val()

    window.nodes = nodes;
    nodes.each(function (i, el) {
      
      var href = el.textContent;
            // remove enclosing " 
            if (href.indexOf('"') == 0) href = href.substring(1, href.length - 1);

            // only add apy key in same domain or relative links

            if (href.search(/[a-z]+:/) != 0 || href.search(document.location.origin) == 0) { 
                // add api key
                href = href + (el.textContent.indexOf("?") > 0 ? "&" : "?") + apiParam;
              }
              
              $(el).replaceWith($("<a href='" + href + "'>" + el.textContent + "</a>"))

            });
  }
}