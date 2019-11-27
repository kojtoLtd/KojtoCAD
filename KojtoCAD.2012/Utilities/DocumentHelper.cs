using Castle.Core.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
using Exception = Teigha.Runtime.Exception;
using TransactionManager = Teigha.DatabaseServices.TransactionManager;
#endif

namespace KojtoCAD.Utilities
{
    public class DocumentHelper
    {
        private readonly Document _doc;
        private readonly Editor _ed;
        private readonly Database _db;
        private readonly LayerManipulator _layerManipulator; 
        
        private ILogger _logger = NullLogger.Instance;

        public DocumentHelper(Document document)
        {
            _logger = IoC.ContainerRegistrar.Container.Resolve<ILogger>();
            if (document == null)
            {
                _logger.Fatal("Document provided to Drawing Helper is null.");
                throw new ArgumentNullException("Ducument is null.");
            }
            _layerManipulator = new LayerManipulator(document);
            _doc = document;
            _ed = document.Editor;
            _db = document.Database;
        }

        public TransactionManager TransactionManager
        {
            get { return _db.TransactionManager; }
        }

        public Database Database
        {
            get { return _db; }
        }

        public LayerManipulator LayerManipulator
        {
            get { return _layerManipulator; }
        }

        public IEnumerable<string> GetLayouts()
        {
            var layouts = new List<string>();
            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                var blockTableRecordObjectIds = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForRead);
                var modelSpace = (BlockTableRecord)transaction.GetObject(blockTableRecordObjectIds[BlockTableRecord.ModelSpace], OpenMode.ForRead);
                foreach (ObjectId blockTableRecordObjectId in blockTableRecordObjectIds)
                {
                    var blockTableRecord = (BlockTableRecord)transaction.GetObject(blockTableRecordObjectId, OpenMode.ForRead);
                    if (blockTableRecord.IsLayout && blockTableRecord.ObjectId != modelSpace.ObjectId)
                    {
                        var currLayout = (Layout)transaction.GetObject(blockTableRecord.LayoutId, OpenMode.ForRead);
                        layouts.Add(currLayout.LayoutName);
                    }
                }
            }
            return layouts;
        }

        /// <summary>
        /// Creates BlockReference from the block with the given name. 
        /// Throws ArgumentException if block with such a name does not exist.
        /// </summary>
        /// <param name="blockName"></param>
        /// <param name="insertionPoint"></param>
        /// <param name="space">The model space or some of the paper spaces</param>
        /// <param name="blockTable">The block table of the associated drawing in the helper.</param>
        /// <returns>The BlockReference of the block</returns>
        private BlockReference CreateBlockReference(string blockName,UnitsValue sourceBlockMeasurementUnits, Point3d insertionPoint, BlockTableRecord space, BlockTable blockTable)
        {
            Matrix3d ucs = _ed.CurrentUserCoordinateSystem;
            BlockReference newBlockReference;

            //All open objects opened during a transaction are closed at the end of the transaction.
            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                blockTable.UpgradeOpen();

                // If the DWG already contains this block definition we will create a block reference and not a copy of the same definition 
                if (!blockTable.Has(blockName))
                {
                    _logger.Error(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name + " : Block with name '" + blockName + "' does not exist.");
                    transaction.Abort();
                    throw new ArgumentException("Block with name '" + blockName + "' does not exist.");
                }

                BlockTableRecord sourceBlockTableRecord = (BlockTableRecord)transaction.GetObject(blockTable[blockName], OpenMode.ForRead);

                newBlockReference = new BlockReference(insertionPoint, sourceBlockTableRecord.ObjectId);

                var converter = new MeasurementUnitsConverter();

                var scaleFactor = converter.GetScaleRatio(sourceBlockMeasurementUnits, blockTable.Database.Insunits);

                _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                newBlockReference.TransformBy(ucs);
                _ed.CurrentUserCoordinateSystem = ucs;

                newBlockReference.ScaleFactors = new Scale3d(scaleFactor);

                space.UpgradeOpen();
                space.AppendEntity(newBlockReference);
                transaction.AddNewlyCreatedDBObject(newBlockReference, true);

                AttributeCollection atcoll = newBlockReference.AttributeCollection;
                foreach (ObjectId subid in sourceBlockTableRecord)
                {
                    var entity = (Entity)subid.GetObject(OpenMode.ForRead);
                    var attributeDefinition = entity as AttributeDefinition;

                    if (attributeDefinition != null)
                    {
                        var attributeReference = new AttributeReference();

                        attributeReference.SetPropertiesFrom(attributeDefinition);
                        attributeReference.Visible = attributeDefinition.Visible;
                        attributeReference.SetAttributeFromBlock(attributeDefinition, newBlockReference.BlockTransform);
                        attributeReference.HorizontalMode = attributeDefinition.HorizontalMode;
                        attributeReference.VerticalMode = attributeDefinition.VerticalMode;
                        attributeReference.Rotation = attributeDefinition.Rotation;
                        attributeReference.Position = attributeDefinition.Position + insertionPoint.GetAsVector();
                        attributeReference.Tag = attributeDefinition.Tag;
                        attributeReference.FieldLength = attributeDefinition.FieldLength;
                        attributeReference.TextString = attributeDefinition.TextString;
                        attributeReference.AdjustAlignment(_db);
                        atcoll.AppendAttribute(attributeReference);
                        transaction.AddNewlyCreatedDBObject(attributeReference, true);
                    }
                }
                transaction.Commit();
            }
            _ed.Regen();

            return newBlockReference;
        }

        public ObjectId ImportDynamicBlockAndFillItsProperties(string dynamicBlockPath, Point3d basePoint,
            Hashtable dynamicBlockProperties, Hashtable dynamicBlockAttributes)
        {
            var resultingObjectId = new ObjectId();

            var blockname = dynamicBlockPath.Remove(0, dynamicBlockPath.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            blockname = blockname.Remove(blockname.LastIndexOf(".dwg", StringComparison.Ordinal));

            using (_doc.LockDocument())
            {
                using (var inMemoryDb = new Database(false, true))
                {
                    inMemoryDb.ReadDwgFile(dynamicBlockPath, System.IO.FileShare.Read, true, "");
                    using (var transaction = _doc.TransactionManager.StartTransaction())
                    {
                        var blockTable = (BlockTable) transaction.GetObject(_db.BlockTableId, OpenMode.ForRead);

                        AttributeCollection atcoll;
                        if (blockTable.Has(blockname))
                        {
                            var currentSpace = (BlockTableRecord) _db.CurrentSpaceId.GetObject(OpenMode.ForWrite);
                            var newDynamicBlockDefinition = (BlockTableRecord) transaction.GetObject(blockTable[blockname], OpenMode.ForRead);

                            // Create a block reference to the existing block definition
                            var newBlockReference = new BlockReference(basePoint, newDynamicBlockDefinition.ObjectId);
                            newBlockReference.TransformBy(Matrix3d.Identity);
                            newBlockReference.ScaleFactors = new Scale3d(1, 1, 1);
                            currentSpace.AppendEntity(newBlockReference);
                            transaction.AddNewlyCreatedDBObject(newBlockReference, true);

                            resultingObjectId = newBlockReference.ObjectId;

                            using (var pc = newBlockReference.DynamicBlockReferencePropertyCollection)
                            {
                                foreach (DynamicBlockReferenceProperty prop in pc)
                                {
                                    if (dynamicBlockProperties.ContainsKey(prop.PropertyName))
                                    {
                                        prop.Value = dynamicBlockProperties[prop.PropertyName];
                                    }
                                }
                            }

                            var bref3 =
                                (BlockReference) transaction.GetObject(newBlockReference.ObjectId, OpenMode.ForWrite);
                            atcoll = bref3.AttributeCollection;
                            foreach (var subid in newDynamicBlockDefinition)
                            {
                                var ent = (Entity) subid.GetObject(OpenMode.ForRead);
                                var attDef = ent as AttributeDefinition;
                                if (attDef == null)
                                {
                                    continue;
                                }
                                var attRef = new AttributeReference();
                                attRef.SetPropertiesFrom(attDef);
                                attRef.SetAttributeFromBlock(attDef, newBlockReference.BlockTransform);
                                attRef.AdjustAlignment(_db);
                                attRef.TextString = dynamicBlockAttributes.ContainsKey(attRef.Tag)
                                    ? dynamicBlockAttributes[attRef.Tag].ToString()
                                    : attDef.TextString;
                                atcoll.AppendAttribute(attRef);
                                transaction.AddNewlyCreatedDBObject(attRef, true);
                            }

                            transaction.Commit();
                            _ed.Regen();
                            return resultingObjectId;
                        }
                        // There is not such block definition, so we are inserting/creating new one
                        var sourceBlockId = _db.Insert(blockname, inMemoryDb, false);

                        // We continue the creation of the new block definition of the sourceDWG
                        var btrec = (BlockTableRecord) sourceBlockId.GetObject(OpenMode.ForRead);
                        btrec.UpgradeOpen();
                        btrec.Name = blockname;
                        btrec.DowngradeOpen();

                        var currentSpaceBlockTableRecord = (BlockTableRecord) _db.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                        // We have created the block definition up there, and now we create the block reference to this block definition
                        var bref = new BlockReference(basePoint, sourceBlockId);
                        currentSpaceBlockTableRecord.AppendEntity(bref);
                        transaction.AddNewlyCreatedDBObject(bref, true);
                        if (bref.IsDynamicBlock)
                        {
                            resultingObjectId = bref.ObjectId;
                            using (var pc = bref.DynamicBlockReferencePropertyCollection)
                            {
                                try
                                {
                                    foreach (DynamicBlockReferenceProperty prop in pc)
                                    {
                                        if (dynamicBlockProperties.ContainsKey(prop.PropertyName))
                                        {
                                            prop.Value = dynamicBlockProperties[prop.PropertyName];
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    _logger.Error("Error applying dynamic block properties.", exception);
                                    throw;
                                }

                            }
                        }

                        // Copy the attributes
                        var btAttRec = (BlockTableRecord) bref.BlockTableRecord.GetObject(OpenMode.ForWrite);
                        var bref2 = (BlockReference) transaction.GetObject(bref.ObjectId, OpenMode.ForWrite);
                        atcoll = bref2.AttributeCollection;
                        foreach (var subid in btAttRec)
                        {
                            var ent = (Entity) subid.GetObject(OpenMode.ForRead);
                            var attDef = ent as AttributeDefinition;

                            if (attDef == null)
                            {
                                continue;
                            }
                            var attRef = new AttributeReference();
                            attRef.SetPropertiesFrom(attDef);
                            attRef.SetAttributeFromBlock(attDef, bref.BlockTransform);
                            attRef.AdjustAlignment(_db);
                            attRef.TextString = dynamicBlockAttributes.ContainsKey(attRef.Tag)
                                ? dynamicBlockAttributes[attRef.Tag].ToString()
                                : attDef.TextString;

                            atcoll.AppendAttribute(attRef);

                            transaction.AddNewlyCreatedDBObject(attRef, true);
                        }
                        
                        transaction.Commit();
                    }
                    _ed.Regen();
                }
            }
            return resultingObjectId;
        }

        public ObjectId AddEntityDefinitionToCurrentSpace(Entity entity, Transaction tr)
        {
            var currentSpace = (BlockTableRecord) tr.GetObject(_db.CurrentSpaceId, OpenMode.ForWrite);
            currentSpace.AppendEntity(entity);
            tr.AddNewlyCreatedDBObject(entity, true);
            return entity.ObjectId;
        }

        /// <summary>
        /// the source drawig should be drawn as number of
        /// separate entites with or without attributes.
        /// Throws NotImplementedException if invoked with .dxf file
        /// </summary>
        /// <param name="sourceDrawing"></param>
        /// <param name="insertionPoint"></param>
        /// <returns>ObjectID of the Block Def that was imported.</returns>
        public void ImportDwgAsBlock(string sourceDrawing, Point3d insertionPoint)
        {
            Matrix3d ucs = _ed.CurrentUserCoordinateSystem;

            string blockname = sourceDrawing.Remove(0, sourceDrawing.LastIndexOf("\\", StringComparison.Ordinal) + 1);
            blockname = blockname.Substring(0, blockname.Length - 4); // remove the extension

            try
            {
                using (_doc.LockDocument())
                {
                    using (var inMemoryDb = new Database(false, true))
                    {
                        #region Load the drawing into temporary inmemory database
                        if (sourceDrawing.LastIndexOf(".dwg", StringComparison.Ordinal) > 0)
                        {
                            inMemoryDb.ReadDwgFile(sourceDrawing, System.IO.FileShare.Read, true, "");
                        }
                        else if (sourceDrawing.LastIndexOf(".dxf", StringComparison.Ordinal) > 0)
                        {
                            _logger.Error(MethodBase.GetCurrentMethod().DeclaringType.FullName + "." + MethodBase.GetCurrentMethod().Name + " : Tried to invoke the method with .dxf file.");
                            throw new NotImplementedException("Importing .dxf is not supported in this version.");
                            //inMemoryDb.DxfIn("@" + sourceDrawing, System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + "\\log\\import_block_dxf_log.txt");
                        }
                        else
                        {
                            throw new ArgumentException("This is not a valid drawing.");
                        }
                        #endregion
                                                     
                        using (var transaction =  _db.TransactionManager.StartTransaction())
                        {
                            BlockTable destDbBlockTable = (BlockTable)transaction.GetObject(_db.BlockTableId, OpenMode.ForRead);
                            BlockTableRecord destDbCurrentSpace = (BlockTableRecord)_db.CurrentSpaceId.GetObject(OpenMode.ForWrite);

                            // If the destination DWG already contains this block definition
                            // we will create a block reference and not a copy of the same definition
                            ObjectId sourceBlockId;
                            if (destDbBlockTable.Has(blockname))
                            {

                                //BlockTableRecord destDbBlockDefinition = (BlockTableRecord)transaction.GetObject(destDbBlockTable[blockname], OpenMode.ForRead);
                                //sourceBlockId = destDbBlockDefinition.ObjectId;

                                sourceBlockId = transaction.GetObject(destDbBlockTable[blockname], OpenMode.ForRead).ObjectId;

                                // Create a block reference to the existing block definition
                                using (var blockReference = new BlockReference(insertionPoint, sourceBlockId))
                                {
                                    _ed.CurrentUserCoordinateSystem = Matrix3d.Identity;
                                    blockReference.TransformBy(ucs);
                                    _ed.CurrentUserCoordinateSystem = ucs;
                                    var converter = new MeasurementUnitsConverter();
                                    var scaleFactor = converter.GetScaleRatio(inMemoryDb.Insunits, _db.Insunits);
                                    blockReference.ScaleFactors = new Scale3d(scaleFactor);
                                    destDbCurrentSpace.AppendEntity(blockReference);
                                    transaction.AddNewlyCreatedDBObject(blockReference, true);
                                    _ed.Regen();
                                    transaction.Commit();
                                    // At this point the Bref has become a DBObject and (can be disposed) and will be disposed by the transaction
                                }
                                return;
                            }

                            //else // There is not such block definition, so we are inserting/creating new one

                            sourceBlockId = _db.Insert(blockname, inMemoryDb, true);
                            BlockTableRecord sourceBlock = (BlockTableRecord)sourceBlockId.GetObject(OpenMode.ForRead);
                            sourceBlock.UpgradeOpen();
                            sourceBlock.Name = blockname;
                            destDbCurrentSpace.DowngradeOpen();
                            var sourceBlockMeasurementUnits = inMemoryDb.Insunits;
                            try
                            {
                                CreateBlockReference(sourceBlock.Name, sourceBlockMeasurementUnits,
                                                     insertionPoint,
                                                     destDbCurrentSpace,
                                                     destDbBlockTable);
                            }
                            catch (ArgumentException argumentException)
                            {
                                _logger.Error("Error. Check inner exception.", argumentException);
                            }

                            _ed.Regen();
                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error("Error in ImportDrawingAsBlock().", exception);
            }

        }

        /// <summary>
        /// This is a managed workaround for getting a non-erased SymbolTableRecord, 
        /// when there are also erased, but still existing (Database resident) ones with the same name.
        /// </summary>
        /// <param name="symbolTableId"></param>
        /// <param name="entityName"></param>
        /// <returns>Returns ObjectId or Null if there is no such entity.</returns>
        public ObjectId GetErasedButResidentTableRecordId(ObjectId symbolTableId, string entityName)
        {
            ObjectId id = ObjectId.Null;
            using (Transaction transaction = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                SymbolTable symbolTable = (SymbolTable)transaction.GetObject(symbolTableId, OpenMode.ForRead);
                if (symbolTable.Has(entityName))
                {
                    id = symbolTable[entityName];
                    if (id.IsErased)
                    {
                        foreach (ObjectId recId in symbolTable)
                        {
                            if (!recId.IsErased)
                            {
                                SymbolTableRecord symbolTableRecord = (SymbolTableRecord)transaction.GetObject(recId, OpenMode.ForRead);
                                if (String.Compare(symbolTableRecord.Name, entityName, StringComparison.OrdinalIgnoreCase) == 0)
                                {
                                    id = recId;
                                }
                            }
                        }
                    }
                    //if (!id.IsErased)
                    //{
                    //    return id;
                    //}

                    //foreach (ObjectId recId in symbolTable)
                    //{
                    //    if (!recId.IsErased)
                    //    {
                    //        SymbolTableRecord symbolTableRecord = (SymbolTableRecord)transaction.GetObject(recId, OpenMode.ForRead);
                    //        if (String.Compare(symbolTableRecord.Name, entityName, StringComparison.OrdinalIgnoreCase) == 0)
                    //        {
                    //            return recId;
                    //        }
                    //    }
                    //}

                }
            }
            return id;
        }

        public StringCollection GetDimensionStylesList()
        {
            StringCollection dimStylesCollection = new StringCollection();
            using (Transaction transaction = _db.TransactionManager.StartTransaction())
            {
                DimStyleTable dimStyleTable = (DimStyleTable)transaction.GetObject(_db.DimStyleTableId, OpenMode.ForRead);
                foreach (ObjectId dimStyleTableRecordId in dimStyleTable)
                {
                    DimStyleTableRecord dimStyleTableRecord = (DimStyleTableRecord)transaction.GetObject(dimStyleTableRecordId, OpenMode.ForRead);
                    dimStylesCollection.Add(dimStyleTableRecord.Name);
                }
                transaction.Commit();
            }
            return dimStylesCollection;
        }
    }
} 