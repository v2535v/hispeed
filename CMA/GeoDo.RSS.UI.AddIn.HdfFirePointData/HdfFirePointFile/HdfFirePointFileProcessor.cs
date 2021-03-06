﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeoDo.RSS.Core.UI;
using System.ComponentModel.Composition;
using GeoDo.RSS.Core.DrawEngine;
using GeoDo.RSS.Core.RasterDrawing;
using GeoDo.RSS.Layout;
using System.IO;
using GeoDo.RSS.UI.AddIn.CanvasViewer;
using GeoDo.RSS.Layout.GDIPlus;
using System.Text.RegularExpressions;

namespace GeoDo.RSS.MIF.Prds.FIR
{
    [Export(typeof(IOpenFileProcessor)), ExportMetadata("VERSION", "1")]
    internal class HdfFirePointFileProcessor : OpenFileProcessor
    {
        public HdfFirePointFileProcessor()
            : base()
        {
            _extNames.Add(".HDF");
        }

        protected override bool FileHeaderIsOK(string fname, string extName)
        {
            if (extName.ToUpper() != ".HDF")
                return false;
            if (Regex.IsMatch(fname, "_ORBT_L2_GFR_MLT_NUL_"))
                return true;
            return false;
        }

        public override bool Open(string fname, out bool memoryIsNotEnough)
        {
            memoryIsNotEnough = false;
            //if (!MemoryIsEnoughChecker.MemoryIsEnouggWithMsgBoxForVector(fname))
            //{
            //    memoryIsNotEnough = true;
            //    return false;
            //}
            if (_session.SmartWindowManager.ActiveViewer is ICanvasViewer)
                AddVectorDataToCanvasViewer(fname,true);
            else if (_session.SmartWindowManager.ActiveViewer is ILayoutViewer)
                AddVectorDataToLayoutViewer(fname,true);
            else
            {
                CanvasViewer cv = new CanvasViewer(OpenFileFactory.GetTextByFileName(fname), _session);
                _session.SmartWindowManager.DisplayWindow(cv);
                IVectorHostLayer host = cv.Canvas.LayerContainer.VectorHost as IVectorHostLayer;
                if (host != null)
                {
                    host.Set(cv.Canvas);
                    AddVectorDataToCanvasViewer(fname, false);
                    CodeCell.AgileMap.Core.IMap map = host.Map as CodeCell.AgileMap.Core.IMap;
                    CodeCell.AgileMap.Core.FeatureLayer fetL = map.LayerContainer.Layers[0] as CodeCell.AgileMap.Core.FeatureLayer;
                    CodeCell.AgileMap.Core.FeatureClass fetc = fetL.Class as CodeCell.AgileMap.Core.FeatureClass;
                    CodeCell.AgileMap.Core.Envelope evp = fetc.FullEnvelope.Clone() as CodeCell.AgileMap.Core.Envelope;
                    CoordEnvelope cvEvp = new CoordEnvelope(evp.MinX, evp.MaxX, evp.MinY, evp.MaxY);
                    cv.Canvas.CurrentEnvelope = cvEvp;
                    cv.Canvas.Refresh(enumRefreshType.All);
                }
            }
            RefreshLayerManager();
            return true;
        }

        public void AddVectorDataToCanvasViewer(string fname, bool isNeedCheckNormalImage,params object[] options)
        {
            ICanvasViewer v = _session.SmartWindowManager.ActiveCanvasViewer;
            if (v == null)
                return;
            ICanvas c = v.Canvas;
            if (c == null)
                return;
            IVectorHostLayer host = c.LayerContainer.VectorHost as IVectorHostLayer;
            if (host == null)
                return;
            AddToCanvas(c, isNeedCheckNormalImage, host, fname);
        }

        public void AddVectorDataToLayoutViewer(string fname, bool isNeedCheckNormalImage, params object[] options)
        {
            ILayoutViewer v = _session.SmartWindowManager.ActiveViewer as ILayoutViewer;
            if (v == null)
                return;
            IDataFrame df = v.LayoutHost.ActiveDataFrame;
            if (df == null)
                return;
            IDataFrameDataProvider provider = df.Provider as IDataFrameDataProvider;
            if (provider == null)
                return;
            ICanvas c = provider.Canvas;
            if (c == null)
                return;
            IVectorHostLayer host = c.LayerContainer.VectorHost as IVectorHostLayer;
            if (host == null)
                return;
            AddToCanvas(c, isNeedCheckNormalImage, host, fname);
            //
            c.Refresh(enumRefreshType.All);
            //
            (v.LayoutHost).SetActiveDataFrame2CurrentTool();
            (v.LayoutHost).Render(true);
        }

        private void AddToCanvas(ICanvas c, bool isNeedCheckNormalImage, IVectorHostLayer host, string filename)
        {
            if (isNeedCheckNormalImage)
            {
                //无坐标的图片
                if (c.IsRasterCoord)
                    return;
            }
            TryCreateOrbitProjection(c);
            string extName = Path.GetExtension(filename).ToUpper();
            switch (extName)
            {
                case ".HDF":
                    host.AddData(filename, null);
                    break;
            }
            c.Refresh(enumRefreshType.All);
        }

        private void TryCreateOrbitProjection(ICanvas c)
        {
            try
            {
                IRasterDrawing drawing = c.PrimaryDrawObject as IRasterDrawing;
                if (drawing == null)
                    return;
                drawing.TryCreateOrbitPrjection();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
