using Gtk;

namespace Clingies.Gtk.Windows
{
    public class ClingyCssProvider : CssProvider
    {
        public ClingyCssProvider()
        {
            var css = @"
            /* Window + base */
            #clingy-window {
            background: #FFFFB4;            /* note body color */
            }

            /* Header bar */
            #clingy-title {
            background: #87CEFA;            /* blue header */
            padding: 4px 6px;
            border: none;
            box-shadow: none;
            }

            /* Title text field in header */
            entry#clingy-title-entry {
            background: transparent;
            color: white;                    /* white text on blue */
            border: none;
            box-shadow: none;
            caret-color: white;
            }

            /* Close button */
            #btn-close {
            background: transparent;
            border: none;
            box-shadow: none;
            color: black;
            }
            #btn-close:hover {
            background: rgba(0,0,0,0.10);
            }

            /* Scroller + viewport + textview must ALL be painted yellow */
            #clingy-content,
            #clingy-content > viewport,
            #clingy-content-view,
            textview#clingy-content-view text,
            textview#clingy-content-view view {
            background: #FFFFB4;
            border: none;
            box-shadow: none;
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
            ";
            base.LoadFromData(css);
        }
    }
}