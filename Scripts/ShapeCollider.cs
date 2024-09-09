using Godot;
using System;
using System.Diagnostics;

public static class ShapeCollider
{
		public struct ColliderRectangle
		{
			public double CenterX, CenterY, Width, Height, Angle;
			
			public ColliderRectangle(Vector2 center, double width, double height, double angle) : this(center.X,
				center.Y, width, height, angle)
			{
			}

			public ColliderRectangle(double centerX, double centerY, double width, double height, double angle)
			{
				CenterX = centerX;
				CenterY = centerY;
				Width = width;
				Height = height;
				Angle = angle;
			}
		}

		private static double[,] GetVertices(ColliderRectangle rect)
		{
			double angle = rect.Angle * Math.PI / 180.0;
			double dx = rect.Width / 2;
			double dy = rect.Height / 2;

			double[,] corners =
			{
				{ -dx, -dy },
				{ dx, -dy },
				{ dx, dy },
				{ -dx, dy }
			};

			double cos = Math.Cos(angle);
			double sin = Math.Sin(angle);

			double[,] rotatedCorners = new double[4, 2];
			for (int i = 0; i < 4; i++)
			{
				rotatedCorners[i, 0] = corners[i, 0] * cos - corners[i, 1] * sin + rect.CenterX;
				rotatedCorners[i, 1] = corners[i, 0] * sin + corners[i, 1] * cos + rect.CenterY;
			}

			return rotatedCorners;
		}

		private static double[] Project(double[,] vertices, double[] axis)
		{
			double min = double.MaxValue;
			double max = double.MinValue;

			for (int i = 0; i < 4; i++)
			{
				double dot = vertices[i, 0] * axis[0] + vertices[i, 1] * axis[1];
				if (dot < min) min = dot;
				if (dot > max) max = dot;
			}

			return new double[] { min, max };
		}

		private static bool Overlap(double[] proj1, double[] proj2)
		{
			return !(proj1[1] < proj2[0] || proj2[1] < proj1[0]);
		}

		/// <summary>
		/// Determines if two rectangles overlaps.
		/// Rectangles may be rotated
		/// </summary>
		/// <param name="rect1">Rectangle 1</param>
		/// <param name="rect2">Rectangle 2</param>
		/// <returns>true = overlap</returns>
		public static bool DoRotatedRectanglesOverlap(ColliderRectangle rect1, ColliderRectangle rect2)
		{
			double[,] vertices1 = GetVertices(rect1);
			double[,] vertices2 = GetVertices(rect2);

			double[][] axes =
			{
				new double[] { vertices1[1, 0] - vertices1[0, 0], vertices1[1, 1] - vertices1[0, 1] },
				new double[] { vertices1[3, 0] - vertices1[0, 0], vertices1[3, 1] - vertices1[0, 1] },
				new double[] { vertices2[1, 0] - vertices2[0, 0], vertices2[1, 1] - vertices2[0, 1] },
				new double[] { vertices2[3, 0] - vertices2[0, 0], vertices2[3, 1] - vertices2[0, 1] }
			};

			foreach (var axis in axes)
			{
				double length = Math.Sqrt(axis[0] * axis[0] + axis[1] * axis[1]);
				axis[0] /= length;
				axis[1] /= length;

				double[] proj1 = Project(vertices1, axis);
				double[] proj2 = Project(vertices2, axis);

				if (!Overlap(proj1, proj2))
				{
					return false;
				}
			}

			return true;
		}

		/*
		public static void Main()
		{
			ColliderRectangle rectA = new ColliderRectangle(0, 0, 2, 2, 45);
			ColliderRectangle rectB = new ColliderRectangle(1, 1, 2, 2, 45);

			Console.WriteLine(DoRotatedRectanglesOverlap(rectA, rectB)); // Output: True or False
		}
	*/

		public static bool CircleRectangleOverlap(double radius, Vector2 circleCenter, Vector2 rectangleCenter, double W, double H,
			double theta)
		{
			return CircleRectangleOverlap(radius, circleCenter.X, circleCenter.Y, rectangleCenter.X, rectangleCenter.Y, H, W,
				theta);
		}

		/// <summary>
		/// Determines if a circle and rectangle overlap
		/// </summary>
		/// <param name="radius">Circle radius</param>
		/// <param name="circleX">Circle center X</param>
		/// <param name="circleY">Circle center Y</param>
		/// <param name="rectX">Rectangle center X</param>
		/// <param name="rectY">Rectangle center Y</param>
		/// <param name="W">Rectangle width</param>
		/// <param name="H">Rectangle height</param>
		/// <param name="theta">Rectangle angle</param>
		/// <returns>true = overlap</returns>
		public static bool CircleRectangleOverlap(double radius, double circleX, double circleY, double rectX, double rectY, double W, double H, double theta)
		{
			// Rotate the circle's center back by -theta
			double cosTheta = Math.Cos(-theta);
			double sinTheta = Math.Sin(-theta);
		
			double XcPrime = cosTheta * (circleX - rectX) - sinTheta * (circleY - rectY) + rectX;
			double YcPrime = sinTheta * (circleX - rectX) + cosTheta * (circleY - rectY) + rectY;
		
			// Define the axis-aligned rectangle's corners
			double X1 = rectX - W / 2;
			double Y1 = rectY - H / 2;
			double X2 = rectX + W / 2;
			double Y2 = rectY + H / 2;
		
			// Find the closest point on the rectangle to the circle's center
			double Xn = Math.Max(X1, Math.Min(XcPrime, X2));
			double Yn = Math.Max(Y1, Math.Min(YcPrime, Y2));
		
			// Calculate the distance from the closest point to the circle's center
			double Dx = Xn - XcPrime;
			double Dy = Yn - YcPrime;
			double distance = Math.Sqrt(Dx * Dx + Dy * Dy);
		
			// Check if the distance is less than or equal to the circle's radius
			return distance <= radius;
		}

		/// <summary>
		/// Checks if two circles overlap
		/// </summary>
		/// <param name="r1">Circle 1 radius</param>
		/// <param name="center1">Circle 1 center</param>
		/// <param name="r2">Circle 2 radius</param>
		/// <param name="center2">Circle 2 center</param>
		/// <returns>true = overlap</returns>
		public static bool CirclesOverlap(double r1, Vector2 center1, double r2, Vector2 center2)
		{
			//if the distance between the centers is < the sum of the radii, we collide
			return (center2 - center1).Length() < (r1 + r2) ;
		}

}
