using NUnit.Framework;
using Unity.GraphToolsFoundation.Editor;
using UnityEditor.ShaderGraph.GraphDelta;
using UnityEngine.TestTools;

namespace UnityEditor.ShaderGraph.GraphUI.UnitTests.DataModel
{
    class SGNodeModelTest : BaseGraphAssetTest
    {
        (NodeHandler, SGNodeModel) MakeNode()
        {
            var nodeHandler = GraphModel.GraphHandler.AddNode(new RegistryKey {Name = "Add", Version = 1}, "Test");
            var node = GraphModel.CreateNode<SGNodeModel>("Test", initializationCallback: nm => nm.graphDataName = "Test");
            return (nodeHandler, node);
        }

        SGNodeModel MakeOrphanNode()
        {
            return GraphModel.CreateNode<SGNodeModel>("Test", spawnFlags: SpawnFlags.Orphan, initializationCallback: nm => nm.SetSearcherPreviewRegistryKey(new RegistryKey { Name = "Add", Version = 1 }));
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
    }
}
