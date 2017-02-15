using System;
#if !bcad
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
#else
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Bricscad.EditorInput;
#endif

#if acad2013
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
#endif
#if acad2012
using Application = Autodesk.AutoCAD.ApplicationServices.Application;
#endif
#if bcad
using Application = Bricscad.ApplicationServices.Application;
#endif

namespace KojtoCAD.Utilities
{
  public class RectangleJig : EntityJig
  {
    private Point2d P1, P2, P3, P4;
    private Point3d corner1, corner2;
    private PromptPointResult pres;
    public Polyline Polyline;
    private double tan30 = Math.Tan( (30.0 / 180.0) * Math.PI );
    private double degAng2Use1;
    private double degAng2Use2;

    public RectangleJig ( Point3d _corner1 )   : base( new Polyline( 3 ) )
    {
      corner1 = _corner1;
      Polyline = ( Polyline ) Entity;
      Polyline.SetDatabaseDefaults( );
      P1 = new Point2d( corner1.X, corner1.Y );
      Polyline.AddVertexAt( 0, P1, 0, 0, 0 );
      Polyline.AddVertexAt( 1, P1, 0, 0, 0 );
      Polyline.AddVertexAt( 2, P1, 0, 0, 0 );
      Polyline.AddVertexAt( 3, P1, 0, 0, 0 );
      Polyline.Closed = true;
    }

    protected override SamplerStatus Sampler ( JigPrompts prompts )
    {
      JigPromptPointOptions jigPointOpts = new JigPromptPointOptions( "\nSpecify other corner point" );

      jigPointOpts.UseBasePoint = true;
      jigPointOpts.BasePoint = corner1;
      jigPointOpts.UserInputControls = (UserInputControls.Accept3dCoordinates) | UserInputControls.NullResponseAccepted;
      pres = prompts.AcquirePoint( jigPointOpts );
      Point3d endPointTemp = pres.Value;

      if ( endPointTemp != corner2 )
      {
        corner2 = endPointTemp;
      }
      else
        return SamplerStatus.NoChange;

      if ( pres.Status == PromptStatus.Cancel )
        return SamplerStatus.Cancel;
      else
        return SamplerStatus.OK;
    }

    private bool IsometricSnapIsOn ( )
    {
        var snapVarValue = Application.GetSystemVariable("Snapstyl").ToString();
        return snapVarValue == "1";
    }

      private String GetIsoPlane ( )
    {
      String result = "Left";

      if ( Application.GetSystemVariable( "Snapisopair" ).ToString( ) == "1" )
      {
        result = "Top";
      }
      else if ( Application.GetSystemVariable( "Snapisopair" ).ToString( ) == "2" )
      {
        result = "Right";
      }
      return result;
    }

    private Point2d PolarPoint ( Point2d basepoint, double angle, double distance )
    {
      return new Point2d(
      basepoint.X + (distance * Math.Cos( angle )),
      basepoint.Y + (distance * Math.Sin( angle ))
      );
    }

    private Point2d ImaginaryIntersect ( Point2d line1_pt1, Point2d line1_pt2, Point2d line2_pt1, Point2d line2_pt2 )
    {
      Line2d line1 = new Line2d( line1_pt1, line1_pt2 );
      Line2d line2 = new Line2d( line2_pt1, line2_pt2 );

      double line1Ang = line1.Direction.Angle;
      double line2Ang = line2.Direction.Angle;

      double line1ConAng = line1Ang + Degrees_Radians_Conversion( 180, false );
      double line2ConAng = line2Ang + Degrees_Radians_Conversion( 180, false );

      Point2d RayLine1_pt1 = PolarPoint( line1_pt1, line1Ang, 10000 );
      Point2d RayLine1_pt2 = PolarPoint( line1_pt1, line1ConAng, 10000 );
      Line2d RayLine1 = new Line2d( RayLine1_pt1, RayLine1_pt2 );

      Point2d RayLine2_pt1 = PolarPoint( line2_pt1, line2Ang, 10000 );
      Point2d RayLine2_pt2 = PolarPoint( line2_pt1, line2ConAng, 10000 );
      Line2d RayLine2 = new Line2d( RayLine2_pt1, RayLine2_pt2 );

      Point2d[] col = RayLine1.IntersectWith( RayLine2 );

      return col[0];

    }

    private Double Degrees_Radians_Conversion ( Double Angle, bool inputIsRadians )
    {
      if ( inputIsRadians )
      {
        Angle = (180 * (Angle / Math.PI));
      }
      else
      {
        Angle = (Math.PI * (Angle / 180));
      }
      return Angle;
    }

    protected override bool Update ( )
    {
      if ( !IsometricSnapIsOn( ) )
      {
        degAng2Use1 = 0;
        degAng2Use2 = 90;
      }
      else if ( GetIsoPlane( ) == "Right" )
      {
        degAng2Use1 = 30;
        degAng2Use2 = 90;
      }
      else if ( GetIsoPlane( ) == "Left" )
      {
        {
          degAng2Use1 = 330;
          degAng2Use2 = 90;
        }
      }
      else
      {
        {
          degAng2Use1 = 30;
          degAng2Use2 = 330;
        }
      }

      double Ang2Use1 = Degrees_Radians_Conversion( degAng2Use1, false );
      double ConAng2Use1 = Ang2Use1 + Degrees_Radians_Conversion( 180, false );

      double Ang2Use2 = Degrees_Radians_Conversion( degAng2Use2, false );
      double ConAng2Use2 = Ang2Use2 + Degrees_Radians_Conversion( 180, false );



      P3 = new Point2d( corner2.X, corner2.Y );
      //double y = tan30 * (corner2.X - corner1.X);
      P2 = ImaginaryIntersect( P1, PolarPoint( P1, Ang2Use1, 1 ), P3, PolarPoint( P3, Ang2Use2, 1 ) );
      P4 = ImaginaryIntersect( P1, PolarPoint( P1, ConAng2Use2, 1 ), P3, PolarPoint( P3, ConAng2Use1, 1 ) );
      Polyline.SetPointAt( 1, P2 );
      Polyline.SetPointAt( 2, P3 );
      Polyline.SetPointAt( 3, P4 );

      return true;
    }
  }// end class RectangleJig
}
