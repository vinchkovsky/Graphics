using System.Linq;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.ShaderGraph.GraphUI.GraphElements.Toolbars;

namespace UnityEditor.ShaderGraph.GraphUI
{
    class ShaderGraphGraphTool: BaseGraphTool
    {
        public static readonly string toolName = "Shader Graph";

        MainPreviewView m_MainPreviewView;

        PreviewManager m_PreviewManager;

        bool m_WasWindowClosedInDirtyState;

        GraphModelStateObserver m_GraphModelStateObserver;

        // We setup a reference to the MainPreview when the overlay containing it is created
        // We do this because the resources needed to initialize the preview are not available at overlay creation time
        internal void SetMainPreviewReference(MainPreviewView mainPreviewView)
        {
            m_MainPreviewView = mainPreviewView;
        }

        public ShaderGraphGraphTool()
        {
            Name = toolName;
        }

        /// <summary>
        /// Called by the ShaderGraphEditorWindow to initialize with any needed info.
        /// </summary>
        /// <param name="windowCloseDirtyState"> Flag used to inform systems whether to clear dirty state or not </param>
        public void InitTool(bool windowCloseDirtyState)
        {
            m_WasWindowClosedInDirtyState = windowCloseDirtyState;
        }

        /// <summary>
        /// Named differently to not collide with BaseGraphTool.Update()
        /// Responsible for updating any dependent systems in the Shader Graph tool every frame
        /// </summary>
        public void UpdateTool()
        {
            m_PreviewManager?.Update();
        }

        /// <summary>
        /// Gets the toolbar provider for the main toolbar.
        /// </summary>
        /// <remarks>Use this method to get the provider for the <see cref="ShaderGraphMainToolbar"/>.</remarks>
        /// <returns>The toolbar provider for the main toolbar.</returns>
        public override IToolbarProvider GetToolbarProvider()
        {
            return new ShaderGraphMainToolbarProvider();
        }

        protected override IOverlayToolbarProvider CreateToolbarProvider(string toolbarId)
        {
            switch (toolbarId)
            {
                case MainOverlayToolbar.toolbarId:
                    return new ShaderGraphMainToolbarProvider();
                case BreadcrumbsToolbar.toolbarId:
                    return new BreadcrumbsToolbarProvider();
                case PanelsToolbar.toolbarId:
                    return new SGPanelsToolbarProvider();
                case OptionsMenuToolbar.toolbarId:
                    return new OptionsToolbarProvider();
                default:
                    return null;
            }
        }

        void PostModelLoaded(ShaderGraphModel shaderGraphModel)
        {
            var stateComponents = State.AllStateComponents;
            var graphModelStateComponent = (GraphModelStateComponent)stateComponents.First(component => component is GraphModelStateComponent);
            var selectionStateComponent = (SelectionStateComponent)stateComponents.First(component => component is SelectionStateComponent);

            shaderGraphModel.graphModelStateComponent = graphModelStateComponent;
            shaderGraphModel.selectionStateComponent = selectionStateComponent;

            m_PreviewManager = new PreviewManager(graphModelStateComponent, shaderGraphModel, m_MainPreviewView, m_WasWindowClosedInDirtyState);

            if(m_GraphModelStateObserver != null)
                ObserverManager.UnregisterObserver(m_GraphModelStateObserver);
            // Create the Graph Model State Observer and register it
            m_GraphModelStateObserver = new GraphModelStateObserver(graphModelStateComponent, m_PreviewManager, shaderGraphModel.GraphHandler);
            ObserverManager.RegisterObserver(m_GraphModelStateObserver);

            ShaderGraphCommandsRegistrar.RegisterCommandHandlers(this, graphModelStateComponent, m_PreviewManager, shaderGraphModel, Dispatcher);
        }

        protected override void InitDispatcher()
        {
            var shaderGraphCommandDispatcher = new ShaderGraphCommandDispatcher(this);
            Dispatcher = shaderGraphCommandDispatcher;
            shaderGraphCommandDispatcher.OnGraphLoaded += PostModelLoaded;
        }
    }
}
