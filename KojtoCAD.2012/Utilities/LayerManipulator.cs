using System;
using Castle.Core.Logging;
using KojtoCAD.Utilities.ErrorReporting.Exceptions;
#if !bcad
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Exception = Autodesk.AutoCAD.Runtime.Exception;
#else
using Bricscad.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Colors;
using Teigha.Runtime;
using Exception = Teigha.Runtime.Exception;
#endif


namespace KojtoCAD.Utilities
{
    public class LayerManipulator
    {
        private readonly Database _db;
        private ILogger _logger = NullLogger.Instance;

        public ILogger Logger
        {
            get { return _logger; }
            set { _logger = value; }
        }

        public LayerManipulator(Document document)
        {

            _db = document.Database;
        }

        public bool LayerExists(string layerName)
        {
            return (GetLayerObjectIdByName(layerName) != ObjectId.Null);
        }

        public void SetLayerColor(string layerName, System.Drawing.Color aLayerColor)
        {
            var layerId = GetLayerObjectIdByName(layerName);
            if (layerId == ObjectId.Null)
            {
                throw new LayerDoesNotExistException("Layer does not exist.");
            }

            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);
                layerRecord.Color = Color.FromColor(aLayerColor);
                transaction.Commit();
            }
        }

        public void ChangeLayer(string layerName)
        {
            var layerId = GetLayerObjectIdByName(layerName);
            if (layerId.IsNull)
            {
                throw new ArgumentNullException("Layer ObjectId is null.");
            }

            ChangeLayer(layerId);
        }

        /// <summary>
        /// Changes layer. Throws ArgumentNullException if the layer objectId is nonexisting.
        /// </summary>
        /// <param name="layerId"></param>
        public void ChangeLayer(ObjectId layerId)
        {
            if (layerId.IsNull)
            {
                throw new ArgumentNullException("Layer ObjectId is null.");
            }

            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerTableRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForWrite);
                _db.Clayer = layerId;
                transaction.Commit();
            }
        }

        /// <summary>
        /// Returns valid ObjectId or ObjectId.Null if layer does not exist
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns>ObjectId or ObjectId.Null</returns>
        private ObjectId GetLayerObjectIdByName(string layerName)
        {
            if (String.IsNullOrEmpty(layerName))
            {
                throw new ArgumentException("Layername is null or empty.");
            }

            var layerId = ObjectId.Null;
            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(_db.LayerTableId, OpenMode.ForRead);
                try
                {
                    layerId = layerTable[layerName];
                }
                catch (Exception exception)
                {
                    if (exception.ErrorStatus == ErrorStatus.KeyNotFound)
                    {
                        layerId = GetErasedButResidentTableRecordId(layerTable.ObjectId, layerName);
                    }
                }
                return layerId;
            }
        }

        public ObjectId CreateLayer(string layerName, System.Drawing.Color layerColor)
        {
            ObjectId layerId;
            
            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable)transaction.GetObject(_db.LayerTableId, OpenMode.ForRead);
                layerId = GetLayerObjectIdByName(layerName);

                if (layerId.IsNull)
                {
                    var layerRecord = new LayerTableRecord();

                    if (!layerTable.IsWriteEnabled)
                    {
                        layerTable.UpgradeOpen();
                    }

                    layerRecord.Name = layerName;
                    layerRecord.Color = Color.FromColor(layerColor);

                    layerTable.Add(layerRecord);
                    transaction.AddNewlyCreatedDBObject(layerRecord, true);

                    transaction.Commit();
                }

                layerId = GetLayerObjectIdByName(layerName);
            }
            return layerId;
        }

        /// <summary>
        /// Returns layer color. Throws LayerDoesNotExist exception.
        /// </summary>
        /// <param name="layerName"></param>
        /// <returns></returns>
        public System.Drawing.Color GetLayerColor(string layerName)
        {
            System.Drawing.Color color;
            var layerId = GetLayerObjectIdByName(layerName);
            if (layerId == ObjectId.Null)
            {
                throw new LayerDoesNotExistException("Layer does not exist.");
            }

            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerRecord = (LayerTableRecord)transaction.GetObject(layerId, OpenMode.ForRead);
                color = layerRecord.Color.ColorValue;
                transaction.Commit();
            }
            return color;
        }

        public void SetLayerLinetype(string layerName, string lineTypeName, string lineTypeSourceFile)
        {
            using (var transaction = _db.TransactionManager.StartTransaction())
            {
                var layerTable = (LayerTable) transaction.GetObject(_db.LayerTableId, OpenMode.ForRead);

                LayerTableRecord layerTableRecord;

                if (!layerTable.Has(layerName))
                {
                    layerTableRecord = new LayerTableRecord {Name = layerName};

                    layerTable.UpgradeOpen();
                    layerTable.Add(layerTableRecord);
                    transaction.AddNewlyCreatedDBObject(layerTableRecord, true);
                }
                else
                {
                    layerTableRecord = (LayerTableRecord) transaction.GetObject(layerTable[layerName], OpenMode.ForRead, true);
                }

                // Open the Layer table for read
                var lineTbl = (LinetypeTable) transaction.GetObject(_db.LinetypeTableId, OpenMode.ForRead);

                if (lineTbl.Has(lineTypeName))
                {
                    // Upgrade the Layer Table Record for write
                    layerTableRecord.UpgradeOpen();
                    // Set the linetype for the layer
                    layerTableRecord.LinetypeObjectId = lineTbl[lineTypeName];
                }
                else
                {
                    _db.LoadLineTypeFile(lineTypeName, lineTypeSourceFile);

                    if (lineTbl.Has(lineTypeName))
                    {
                        // Upgrade the Layer Table Record for write
                        layerTableRecord.UpgradeOpen();
                        // Set the linetype for the layer
                        layerTableRecord.LinetypeObjectId = lineTbl[lineTypeName];
                    }
                    else
                    {
                        throw new Exception(ErrorStatus.BadLinetypeName, "Linetpye source file is missing or line type name is invalid");
                    }
                }

                // Save the changes and dispose of the transaction
                transaction.Commit();
            }
        }

        public ObjectId GetErasedButResidentTableRecordId(ObjectId symbolTableId, string entityName)
        {
            var id = ObjectId.Null;
            using (var transaction = symbolTableId.Database.TransactionManager.StartTransaction())
            {
                var symbolTable = (SymbolTable)transaction.GetObject(symbolTableId, OpenMode.ForRead);
                if (!symbolTable.Has(entityName))
                {
                    return id;
                }
                if (!id.IsErased)
                {
                    return id;
                }

                foreach (var recId in symbolTable)
                {
                    if (recId.IsErased)
                    {
                        continue;
                    }
                    var symbolTableRecord = (SymbolTableRecord)transaction.GetObject(recId, OpenMode.ForRead);
                    if (string.Compare(symbolTableRecord.Name, entityName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return recId;
                    }
                }
            }
            return id;
        }
    }


}
