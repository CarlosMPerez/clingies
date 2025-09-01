using Gtk;

namespace Clingies.GtkFront.Windows
{
    //TODO - Refactor so each component has its own CssProvider, or so the provider returns different CSS
    //for different components
    public class ClingyCssProvider : CssProvider
    {
        public ClingyCssProvider()
        {
            var css = @"
            /* Window + base */
            #clingy-window {
                background: #FFFFB8;            /* note body color */
            }

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

            /* Scroller + viewport + textview must ALL be painted yellow */
            #clingy-content,
            #clingy-content > viewport,
            #clingy-content-view,
            textview#clingy-content-view view {
                background: #FFFFB8;
                border: none;
                box-shadow: none;
                color: black;                    /* black text on yellow */
                border: none;
                box-shadow: none;
                font-family: monospace;
                font-size: 12px;
            }

            textview#clingy-content-view text {
                background: #FFFFB8;
                border: none;
                box-shadow: none;
                color: black;                    /* black text on yellow */
                border: none;
                box-shadow: none;
                font-family: monospace;
                font-size: 12px;
                caret-color: black;
                -GtkWidget-cursor-color: black;
            }

            /* Remove any separators/borders between header and body */
            #clingy-title, 
            #clingy-content {
                border-top-width: 0;
                border-left-width: 0;
                border-right-width: 0;
                border-bottom-width: 0;
                border-top: none;
                border-bottom: none;
            }

            /* Optional: slimmer scrollbars to blend in */
            #clingy-content scrollbar,
            #clingy-content scrollbar slider {
                background: transparent;
                border: none;
                box-shadow: none;
            }

            /* Icon buttons (pin/lock/close) */
            #btn-pin, #btn-lock, #btn-close {
                background: transparent;
                border: none;
                box-shadow: none;
                padding: 0;
                margin: 0; /* margins are set in code for edge-ness */
            }
            #btn-pin:hover, #btn-lock:hover, #btn-close:hover {
                background: rgba(0,0,0,0.10);
            }
            ";

            base.LoadFromData(css);
        }
    }
}