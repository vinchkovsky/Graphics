using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEngine;

namespace UnityEditor.ShaderGraph.GraphUI.UnitTests
{
    public class TestEditorWindow : ShaderGraphEditorWindow
    {
        protected override GraphView CreateGraphView()
        {
            GraphTool.Preferences.SetInitialSearcherSize(SearcherService.Usage.CreateNode, new Vector2(425, 100), 2.0f);

            var testGraphView = new TestGraphView(this, GraphTool, GraphTool.Name);
            m_PreviewManager = new PreviewManager(testGraphView.GraphViewModel.GraphModelState);
            m_GraphModelStateObserver = new GraphModelStateObserver(testGraphView.GraphViewModel.GraphModelState, m_PreviewManager);
            GraphTool.ObserverManager.RegisterObserver(m_GraphModelStateObserver);

            return testGraphView;
        }
    }
}
