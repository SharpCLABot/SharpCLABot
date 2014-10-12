using System.Web;
using System.Web.Optimization;

namespace SharpCLABot
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js",
                        "~/Scripts/jquery.validate.bootstrap.js"
                        ));

            // Here include all js file in the folder lib
            bundles.Add(new ScriptBundle("~/bundles/codemirror/lib").Include(
                "~/Scripts/codemirror-4.6/lib/codemirror.js"));

            // Here include all js file in the subfolders of mode
            bundles.Add(new ScriptBundle("~/bundles/codemirror/mode").Include(
                "~/Scripts/codemirror-4.6/mode/xml/xml.js",
                "~/Scripts/codemirror-4.6/mode/javascript/javascript.js",
                "~/Scripts/codemirror-4.6/mode/css/css.js",
                "~/Scripts/codemirror-4.6/mode/markdown/markdown.js",
                "~/Scripts/codemirror-4.6/mode/htmlmixed/htmlmixed.js"
                ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-theme.css",
                      "~/Content/docs.min.css",
                      "~/Content/site.css",
                      "~/Scripts/codemirror-4.6/lib/codemirror.css"                
                      ));

            // Set EnableOptimizations to false for debugging. For more information,
            // visit http://go.microsoft.com/fwlink/?LinkId=301862
            BundleTable.EnableOptimizations = true;
        }
    }
}
