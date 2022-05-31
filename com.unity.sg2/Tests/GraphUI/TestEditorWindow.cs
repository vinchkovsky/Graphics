using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.VersionControl;
using UnityEngine;

namespace UnityEditor.ShaderGraph.GraphUI.UnitTests
{
    public class TestEditorWindow : ShaderGraphEditorWindow
    {
        protected override GraphView CreateGraphView()
        {
            GraphTool.Preferences.SetInitialSearcherSize(SearcherService.Usage.CreateNode, new Vector2(425, 100), 2.0f);
            var testGraphView = new TestGraphView(this, GraphTool, GraphTool.Name);
            return testGraphView;
        }
    }
}
