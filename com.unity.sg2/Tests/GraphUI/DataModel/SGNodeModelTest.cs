using NUnit.Framework;
using Unity.GraphToolsFoundation.Editor;
using UnityEditor.ShaderGraph.Defs;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityEditor.ShaderGraph.GraphUI.UnitTests.DataModel
{
    class SGNodeModelTest : BaseGraphAssetTest
    {
        static readonly RegistryKey k_TestKey = new() {Name = "Add", Version = 1};

        (NodeHandler, SGNodeModel) MakeNode()
        {
            var nodeHandler = GraphModel.GraphHandler.AddNode(k_TestKey, "Test");
            var node = GraphModel.CreateNode<SGNodeModel>("Test", initializationCallback: nm => nm.graphDataName = "Test");
            return (nodeHandler, node);
        }

        SGNodeModel MakeOrphanNode(RegistryKey? key = null)
        {
            return GraphModel.CreateNode<SGNodeModel>("Test", spawnFlags: SpawnFlags.Orphan, initializationCallback: nm => nm.SetSearcherPreviewRegistryKey(key ?? k_TestKey));
        }

        SGNodeModel MakeUnboundNode()
        {
            // OnDefineNode will log an error because the created node doesn't have a valid backing
            var ignoreFailing = LogAssert.ignoreFailingMessages;
            LogAssert.ignoreFailingMessages = true;

            var node = GraphModel.CreateNode<SGNodeModel>("Test");

            LogAssert.ignoreFailingMessages = ignoreFailing;

            return node;
        }

        [Test]
        public void TestTryGetNodeHandler_NodeInSearcher_GetsDefaultHandler()
        {
            var nodeModel = MakeOrphanNode();
            Assert.IsTrue(nodeModel.TryGetNodeHandler(out var handler));
            Assert.AreEqual(GraphModel.RegistryInstance.DefaultTopologies.graphDelta, handler.Owner);
        }

        [Test]
        public void TestTryGetNodeHandler_NodeOnGraph_GetsHandler()
        {
            var (nodeHandler, nodeModel) = MakeNode();
            Assert.IsTrue(nodeModel.TryGetNodeHandler(out var retrievedHandler));
            Assert.AreEqual(nodeHandler.ID.FullPath, retrievedHandler.ID.FullPath);
        }

        [Test]
        public void TestTryGetNodeHandler_MissingNode_Fails()
        {
            var nodeModel = MakeUnboundNode();
            Assert.IsFalse(nodeModel.TryGetNodeHandler(out _));
        }

        [Test]
        public void TestExistsInGraphData_NodeInSearcher_IsFalse()
        {
            var nodeModel = MakeOrphanNode();
            Assert.IsFalse(nodeModel.existsInGraphData);
        }

        [Test]
        public void TestExistsInGraphData_NodeOnGraph_IsTrue()
        {
            var (_, nodeModel) = MakeNode();
            Assert.IsTrue(nodeModel.existsInGraphData);
        }

        [Test]
        public void TestExistsInGraphData_MissingNode_IsFalse()
        {
            var nodeModel = MakeUnboundNode();
            Assert.IsFalse(nodeModel.existsInGraphData);
        }

        [Test]
        public void TestGetRegistryKey_NodeOnGraph_MatchesHandler()
        {
            var (nodeHandler, nodeModel) = MakeNode();
            Assert.AreEqual(nodeHandler.GetRegistryKey(), nodeModel.registryKey);
        }

        [Test]
        public void TestGetRegistryKey_NodeInSearcher_IsPreviewKey()
        {
            var nodeModel = MakeOrphanNode(k_TestKey);
            Assert.AreEqual(k_TestKey, nodeModel.registryKey);
        }

        [Test]
        public void TestGetRegistryKey_MissingNode_IsEmptyKey()
        {
            var nodeModel = MakeUnboundNode();
            Assert.AreEqual(default(RegistryKey), nodeModel.registryKey);
        }

        [Test]
        public void TestChangeNodeFunction_NodeOnGraph_WithFunctionField_UpdatesFunctionField()
        {
            var (nodeHandler, nodeModel) = MakeNode();
            nodeHandler.AddField(NodeDescriptorNodeBuilder.SELECTED_FUNCTION_FIELD_NAME, "DefaultFunction", true);

            const string newFunction = "OtherFunction";
            nodeModel.ChangeNodeFunction(newFunction);
            Assert.AreEqual(newFunction, nodeHandler.GetField<string>(NodeDescriptorNodeBuilder.SELECTED_FUNCTION_FIELD_NAME).GetData());
        }

        [Test]
        public void TestChangeNodeFunction_NodeOnGraph_WithoutFunctionField_LogsError()
        {
            var (nodeHandler, nodeModel) = MakeNode();
            Assert.IsNull(nodeHandler.GetField(NodeDescriptorNodeBuilder.SELECTED_FUNCTION_FIELD_NAME));

            nodeModel.ChangeNodeFunction("NotValid");
            LogAssert.Expect(LogType.Error, "Unable to update selected function. Node has no selected function field.");
            Assert.IsNull(nodeHandler.GetField(NodeDescriptorNodeBuilder.SELECTED_FUNCTION_FIELD_NAME));
        }

        [Test]
        public void TestChangeNodeFunction_NodeInSearcher_LogsError()
        {
            var nodeModel = MakeOrphanNode();
            nodeModel.ChangeNodeFunction("NotValid");
            LogAssert.Expect(LogType.Error, "Attempted to change the function of a node that doesn't exist on the graph.");
        }

        [Test]
        public void TestChangeNodeFunction_MissingNode_LogsError()
        {
            var nodeModel = MakeUnboundNode();
            nodeModel.ChangeNodeFunction("NotValid");
            LogAssert.Expect(LogType.Error, "Attempted to change the function of a node that doesn't exist on the graph.");
        }
    }
}
