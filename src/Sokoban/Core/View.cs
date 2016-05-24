﻿using System;
using System.Drawing;

namespace Sokoban
{
    public class View
    {
        const int SPRITES_COUNT = 6;
        static FontFamily LETTERS_FONT_FAMILY = SystemFonts.DefaultFont.FontFamily;
        const float LETTERS_FONT_SCALE = 0.9f;

        static Color COLOR_BACKGROUND = Color.FromArgb(0, 0, 40);
        static SolidBrush BRUSH_BACKGROUND = new SolidBrush(COLOR_BACKGROUND);

        Bitmap screen = null;
        Bitmap sprites = null;
        Graphics g = null;
        Font font = null;
        int z = 20;
        int shift = 5;
        public int CellSizePx { get { return z; } }
        readonly Logic logic;

        public Image Canvas { get { return screen; } }        
        public int Width { get { return screen.Width; } }
        public int Height { get { return screen.Height; } }

        public View(Logic gameLogic)
        {
            logic = gameLogic;
        }

        public void Resize(int cellSizePx)
        {
            if (screen != null)
            {
                screen.Dispose();
                sprites.Dispose();
            }

            z = cellSizePx < 10 ? 10 : cellSizePx;
            shift = z / 4;
            int borderW = z / 2;
            int w = logic.LevelMap.CellsHx * z + borderW;
            int h = logic.LevelMap.CellsVy * z + borderW;
            screen = new Bitmap(w, h);

            font = new Font(LETTERS_FONT_FAMILY, z * LETTERS_FONT_SCALE, GraphicsUnit.Pixel);

            if (g != null)
                g.Dispose();

            g = Graphics.FromImage(screen);

            GenerateSprites();
        }

        private void GenerateSprites()
        {
            sprites = new Bitmap(z * SPRITES_COUNT, z);
            Graphics gs = Graphics.FromImage(sprites);

            // empty
            int sx = 0;
            gs.FillRectangle(BRUSH_BACKGROUND, sx, 0, z, z);

            // wall
            sx += z;
            int dd = z / 15 | 1;
            gs.FillRectangle(Brushes.Red, sx, 0, z, z);
            Pen widePen = new Pen(Brushes.DarkRed, dd);
            gs.DrawLine(widePen, sx, 0, sx + z, 0);
            gs.DrawLine(widePen, sx, z / 2, sx + z, z / 2);

            gs.DrawLine(widePen, sx + z / 4, 0, sx + z / 4, z / 2);
            gs.DrawLine(widePen, sx + z / 4 * 3, 0, sx + z / 4 * 3, z / 2);

            gs.DrawLine(widePen, sx + z / 2, z / 2, sx + z / 2, z);

            // barrel
            sx += z;
            gs.FillRectangle(Brushes.DarkGoldenrod, sx + 1, 1, z - 3, z - 3);
            gs.DrawRectangle(Pens.Yellow, sx + 1, 1, z - 3, z - 3);

            // plate
            sx += z;
            gs.FillRectangle(Brushes.Black, sx, 0, z, z);
            gs.FillPolygon(Brushes.DarkOrange, new Point[]
            {
                new Point (sx, 0 ),
                new Point (sx + z / 4, 0 ),
                new Point (sx, z / 4 ),
            });
            gs.FillPolygon(Brushes.DarkOrange, new Point[]
            {
                new Point (sx + z, z ),
                new Point (sx + z, z - z / 4 ),
                new Point (sx + z - z / 4, z ),
            });
            gs.FillPolygon(Brushes.DarkOrange, new Point[]
            {
                new Point (sx, z ),
                new Point (sx, z - z / 4 ),
                new Point (sx + z - z / 4, 0 ),
                new Point (sx + z, 0 ),
                new Point (sx + z, z / 4 ),
                new Point (sx + z / 4, z )
            });

            // barrel on plate
            sx += z;
            gs.FillRectangle(Brushes.DarkGoldenrod, sx + 1, 1, z - 3, z - 3);
            gs.DrawRectangle(Pens.Yellow, sx + 1, 1, z - 3, z - 3);
            int d = z / 10 | 1;
            Pen fatPen = new Pen(Brushes.Gold, d);
            gs.DrawLine(fatPen, sx + 2, 2, sx + z - 3, z - 3);
            gs.DrawLine(fatPen, sx + 2, z - 3, sx + z - 3, 2);

            // player
            sx += z;
                // body
            gs.FillEllipse(Brushes.Blue, sx + 2, 2, z - 2, z - 2);
                // tail
            gs.FillEllipse(Brushes.Blue, sx, z - z / 2, z / 2, z / 2);
                // eyes
            gs.FillEllipse(Brushes.White, sx + z / 3 + 1, z / 3, z / 5, z / 5);
            gs.FillEllipse(Brushes.White, sx + z / 3 * 2 + 1, z / 3, z / 5, z / 5);
        }

        public void DrawCell(int hx, int vy)
        {
            if (g == null) { return; }

            Rectangle srcRect = new Rectangle(0, 0, z, z);
            Cell cell = logic.CellAt(hx, vy);

            bool strangeCell = false;

            switch (cell)
            {
                case Cell.Empty:
                    srcRect.X = 0;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);
                    break;

                case Cell.Wall:
                    // wall
                    srcRect.X = z;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);
                    break;

                case Cell.Barrel:
                    srcRect.X = z * 2;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);
                    break;

                case Cell.Plate:
                    srcRect.X = z * 3;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);
                    break;

                case Cell.BarrelOnPlate:
                    // plate
                    srcRect.X = z * 3;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);

                    // barrel on plate
                    srcRect.X = z * 4;
                    g.DrawImage(sprites, shift + hx * z, shift + vy * z, srcRect, GraphicsUnit.Pixel);
                    break;
                default:
                    strangeCell = true;
                    break;
            } // switch

            if (strangeCell)
                if ((byte)cell > 7 && (byte)cell < 255)
                {
                    string str = "" + Convert.ToChar(cell);
                    g.DrawString(str, font, Brushes.White, shift + hx * z, shift + vy * z);
                }
        } // DrawCell(int hx, int vy)

        public void DrawPlayer()
        {
            if (g == null) { return; }

            DrawCell(logic.PlayerHx, logic.PlayerVy);

            Rectangle srcRect = new Rectangle(0, 0, z, z);

            srcRect.X = z * 5;
            g.DrawImage(
                sprites,
                shift + logic.PlayerHx * z, shift + logic.PlayerVy * z,
                srcRect, GraphicsUnit.Pixel);
        }

        public void UpdateCells(bool updatePlayer = true)
        {
            if (g == null) { return; }

            foreach(Point p in logic.CellsChanged)
                DrawCell(p.X, p.Y);

            if (updatePlayer)
                DrawPlayer();
        }

        public void DrawField()
        {
            if (g == null) { return; }

            g.Clear(COLOR_BACKGROUND);

            Rectangle srcRect = new Rectangle(0, 0, z, z);
            int chx = logic.LevelMap.CellsHx;
            int cvy = logic.LevelMap.CellsVy;

            for (int vy = 0; vy < cvy; vy++)
                for (int hx = 0; hx < chx; hx++)
                    DrawCell(hx, vy);

            DrawPlayer();
        }
    } // class
}