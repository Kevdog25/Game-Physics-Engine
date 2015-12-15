using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Engine
{
    public static class ErrorReport
    {
        #region Fields
        public static String Report = "";
        #endregion

        #region Private Utilities
        private static StreamWriter OutStream;
        private static String fileName;
        private static String TimeStamp;
        #endregion

        #region Properties
        public static String FileName
        {
            get { return FileName;}
            set 
            {
                fileName = value;
                if (OutStream != null)
                    OutStream.Close();
                OutStream = new StreamWriter(fileName);
            }
        }
        #endregion

        #region Constructor/Destructor
        static ErrorReport()
        {
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Prints the game state data to a file with a time stamp
        /// </summary>
        /// <param name="gameTime">Game Time used to stamp errors</param>
        /// <param name="lines">Array of text to print.</param>
        public static void SubmitReport(GameTime gameTime, String line)
        {
            TimeStamp = gameTime.TotalGameTime.TotalMilliseconds +" ";

            if (Report.Equals(line)) { return;}
            else
            {
                Report = line;
                OutStream.WriteLine(TimeStamp+Report);
            }
        }

        public static void SubmitReport(String line)
        {
                OutStream.WriteLine(line);
        }
        #endregion
    }
}
