using Gtk;

namespace Clingies.GtkFront.Windows
{
    //TODO - Refactor so each component has its own CssProvider, or so the provider returns different CSS
    //for different components
    public class GlobalClingyBaseCssProvider : CssProvider
    {
        public GlobalClingyBaseCssProvider()
        {
            var css = @"
                    /* Structure-only defaults (only colors for TITLE BAR WHICH MUST NOT BE TOUCHED) */
                    /* Header bar */
                    #clingy-title {
                        background: #CACECF;            /* grey header */
                        padding: 2px 2px;
                        border: none;
                        box-shadow: none;
                        background-position: center;
                    }

                    #clingy-title.focused {
                        background: #6ABFED;            /* blue header */
                        padding: 2px 2px;
                        border: none;
                        box-shadow: none;
                        background-position: center;
                    }

                    /* Title text field in header */
                    #clingy-title-label {
                        background: transparent;
                        color: black;                    /* black text on grey */
                        border: none;
                        box-shadow: none;
                        font-family: monospace;
                        font-size: 14px;
                        font-weight: bold;
                    }

                    #clingy-content,
                    #clingy-content > viewport,
                    #clingy-content-view,
                    textview#clingy-content-view view {
                        border: none;
                        box-shadow: none;
                    }

                    textview#clingy-content-view text {
                        border: none;
                        box-shadow: none;
                        caret-color: black; /* harmless default; per-note CSS overrides */
                    }

                    #clingy-title, #clingy-content {
                        border-width: 0;
                        border: none;
                    }

                    #clingy-content scrollbar,
                    #clingy-content scrollbar slider {
                        background: transparent;
                        border: none;
                        box-shadow: none;
                    }

                    #btn-pin, #btn-lock, #btn-close {
                        background: transparent;
                        border: none;
                        box-shadow: none;
                        padding: 0;
                        margin: 0;
                    }
                    #btn-pin:hover, #btn-lock:hover, #btn-close:hover {
                        background: rgba(0,0,0,0.10);
                    }";

            LoadFromData(css);
        }
    }
}