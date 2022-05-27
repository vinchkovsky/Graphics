using System.Collections.Generic;
using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph.GraphUI
{
    public class ShaderGraphView : GraphView
    {
        public ShaderGraphView(
            GraphViewEditorWindow window,
            BaseGraphTool graphTool,
            string graphViewName,
            GraphViewDisplayMode displayMode = GraphViewDisplayMode.Interactive)
            : base(window, graphTool, graphViewName, displayMode)
        {
        }

        protected override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            RemoveContextMenuOption(evt, "Disable Nodes");

            // TODO: (Sai) When GTF reworks the bypass nodes functionality, revisit this
            RemoveContextMenuOption(evt, "Bypass Nodes");
        }

        static void RemoveContextMenuOption(ContextualMenuPopulateEvent evt, string optionName)
        {
            for (var index = 0; index < evt.menu.MenuItems().Count; ++index)
            {
                var menuItem = evt.menu.MenuItems()[index];
                if (menuItem is DropdownMenuAction action && action.name == optionName)
                {
                    evt.menu.RemoveItemAt(index);
                    break;
                }
            }
        }

    //    protected override void CollectCopyableGraphElements(
    //        IEnumerable<IGraphElementModel> elements,
    //        HashSet<IGraphElementModel> elementsToCopySet)
    //    {
    //        var elementsList = elements.ToList();
    //        base.CollectCopyableGraphElements(elementsList, elementsToCopySet);

    //        // Pasting a redirect should also paste an edge to its source node.
    //        foreach (var redirect in elementsList.OfType<RedirectNodeModel>())
    //        {
    //            var incomingEdge = redirect.GetIncomingEdges().FirstOrDefault();
    //            if (incomingEdge != null) elementsToCopySet.Add(incomingEdge);
    //        }
    //    }
    }
}
