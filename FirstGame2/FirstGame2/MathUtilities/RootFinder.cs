using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Engine
{
    public static class RootFinder
    {
        private static double polynomial(double[] coefficients, double in_x)
        {
            double value = coefficients[0];
            int length = coefficients.Length - 1;
            double x = in_x;
            for (int i = 1; i <= length; i++)
            {
                value += x * coefficients[i];
                x *= in_x;
            }
            return value;
        }

        /// <summary>
        /// Computes the coefficients of the remainder of the polynomial synthetic division in_a/in_b
        /// Requires: polynomials of the form {A0,A1,...,An} where Ak is the coefficient on x^k
        /// Handles trailing 0s in the input coefficients.
        /// </summary>
        /// <param name="in_a">Numerator of polynomial division</param>
        /// <param name="in_b">Denominator of polynomial division</param>
        /// <returns>The coefficients of the polynomial that is the remainder of the division. 
        /// Returned array has the same length as numerator</returns>
        private static double[] polyMod(double[] in_a, double[] in_b)
        {
            //to handle the desired input format, in_a and in_b need to be converted.
            double[] a = new double[0];
            double[] b = new double[0];
            #region InputConversion
            for (int i = in_a.Length - 1; i >= 0; i--)
            {
                if (in_a[i] != 0)
                {
                    a = new double[i+1];
                    break;
                }
            }
            for (int i = 0; i < a.Length; i++)
            {
                a[i] = in_a[a.Length - 1 - i];
            }
            for (int i = in_b.Length - 1; i >= 0; i--)
            {
                if (in_b[i] != 0)
                {
                    b = new double[i + 1];
                    break;
                }
            }
            for (int i = 0; i < b.Length; i++)
            {
                b[i] = in_b[b.Length - 1 - i];
            }

            #endregion

            //There are j rows and n+1 columns to the matrix
            //where j is the degree of poly b and n is the degree of poly a
            int j = b.Length-1;
            int n = a.Length-1;
            double[] result = new double[n+1];
            double[] mod = new double[in_a.Length];
            double[][] syntheticDivision = new double[j][];
            //Initialize the synthetic division workspace to 0.
            for (int i = 0; i < j; i++)
            {
                syntheticDivision[i] = new double[n + 1];
            }
            for (int i = 0; i < j; i++)
            {
                for (int l = 1; l < n+2-j; l++)
                {
                    syntheticDivision[i][i + l] = b[i+1];
                }
            }

            result[0] = a[0]/b[0];
            for (int i = 1; i < n+1; i++)
            {
                result[i] = a[i];
                for (int k = 0; k < j; k++)
                {
                    if (i - k - 1 >= 0)
                    {
                        result[i] -= result[i-k-1]*syntheticDivision[k][i];
                    }
                }
                result[i] /= b[0];
            }

            for (int i = 0; i < n+1; i++)
            {
                if (i < j)
                {
                    mod[i] = result[n - i] * b[0];
                }
                else
                {
                    mod[i] = 0;
                }
            }

            return mod;
        }

        /// <summary>
        /// Takes 2 polynomials defined by poly = f[0] + f[1]x +...f[n]x^n
        /// Requires that the leading coefficient is f[n]
        /// </summary>
        /// <param name="a">Numerator of Euclidean division </param>
        /// <param name="b">Denominator</param>
        /// <param name="q">Quotient of division</param>
        /// <param name="r">Polynomial remainder</param>
        static void EuclideanDivision(double[] a, double[] b, out double[] q, out double[] r)
        {
            q = new double[Math.Max(a.Length, b.Length)];
            r = new double[a.Length];
            #region initialize to 0
            for (int i = 0; i < r.Length; i++)
            {
                r[i] = a[i];
            }
            for (int i = 0; i < q.Length; i++)
            {
                q[i] = 0;
            }
            #endregion
            double c = LeadingCoefficient(b);
            int Db = Degree(b);
            int Dr = Degree(r);
            double s;
            while (Dr >= Db)
            {
                s = LeadingCoefficient(r) / c;
                q[Dr - Db] += s;
                for (int i = 0; i <= Db; i++)
                {
                    r[Dr - Db + i] -= s * b[i];
                }
                Dr = Degree(r);
            }
        }

        /// <summary>
        /// Determines the number of real roots in an interval of a polynomial
        /// </summary>
        /// <param name="coefficients">The coefficients to the polynomial ordered {a0,a1,a2...an} for a polynomial of degree n</param>
        /// <param name="range">The range in which the roots are to be found</param>
        /// <returns>returns the number of real roots in the range</returns>
        private static int findNumberOfRealRoots(double[] coefficients, double[] range)
        {
            int numberOfRealRoots = 0;
            //set the order of the polynomial
            int n = coefficients.Length-1;
            while (n > 0 && coefficients[n] == 0)
            {
                n--;
            }
            if (n == 0)
            {
                return 0;
            }
            //Initialize the sturm polynomial sequence holders
            int[] sturmPolySequenceA = new int[n+1];
            int[] sturmPolySequenceB = new int[n+1];
            //Updated coefficients list for the polynomial
            double[] startCoeffs = new double[n+2];
            double[] previousCoeffs = new double[n + 2];
            double[] currentCoeffs;
            for (int i = 0; i < n+2; i++)
            {
                if (i <= n)
                {
                    startCoeffs[i] = coefficients[i];
                }
                else
                {
                    startCoeffs[i] = 0;
                }
            }

            //Initialize the previous coeffs as the derivative of start
            for (int i = 0; i < startCoeffs.Length - 1; i++)
            {
                previousCoeffs[i] = startCoeffs[i + 1] * (i + 1);
            }
            previousCoeffs[n + 1] = 0;

            #region Scale the polynomials to be monic
            int leadingCoeffIndex = 0;
            #endregion
            //Initialize the sturm sequences with the first values. The actual funtion value is of no use
            //only the sign is retained.

            //Get the signs of the first 2 terms of the sequence
            sturmPolySequenceA[0] = Math.Sign(polynomial(coefficients, range[0]));
            sturmPolySequenceB[0] = Math.Sign(polynomial(coefficients, range[1]));
            sturmPolySequenceA[1] = Math.Sign(polynomial(previousCoeffs,range[0]));
            sturmPolySequenceB[1] = Math.Sign(polynomial(previousCoeffs,range[1]));
            
            //Generate the next term recursively, as -(Pn-2 % Pn-1)
            for (int k = 2; k <= n; k++)
            {
                //Get the remainder of the division of Pn-2 and Pn-1
                currentCoeffs = polyMod(startCoeffs,previousCoeffs);
                   
                //Find the leading coefficient
                leadingCoeffIndex = Degree(currentCoeffs);

                //Negate it
                for (int i = 0; i <= leadingCoeffIndex; i++)
                {
                    currentCoeffs[i] *= -1;
                }

                //Get the sign of the polynomials and store it in the sturm Poly sequences
                sturmPolySequenceA[k] = Math.Sign(polynomial(currentCoeffs, range[0]));
                sturmPolySequenceB[k] = Math.Sign(polynomial(currentCoeffs, range[1]));
                startCoeffs = previousCoeffs;
                previousCoeffs = currentCoeffs;
                if (leadingCoeffIndex == 0)
                {
                    break;
                }
            }
            //Count the number of sign flips in each sequence and find the difference.
            numberOfRealRoots += getNumberOfSignSwaps(sturmPolySequenceA);
            numberOfRealRoots -= getNumberOfSignSwaps(sturmPolySequenceB);

            return numberOfRealRoots;
        }

        /// <summary>
        /// Calculates the number of sign swaps in a sequence of signs. This was created to be used for the Sturm polynomial
        /// root determining algorithm and handles 0 as such.
        /// </summary>
        /// <param name="sequence">A sequence of values representing signs [ie. -1,0,1]</param>
        /// <returns>Returns the number of sign swaps</returns>
        private static int getNumberOfSignSwaps(int[] sequence)
        {
            int swaps = 0;
            int lastSign = sequence[0];
            for (int i = 1; i < sequence.Length; i++)
            {
                int nextSign = sequence[i];
                if (nextSign != 0)
                {
                    if (sequence[i - 1] == 0)
                    {
                        if (nextSign == lastSign)
                        {
                            swaps += 2;
                        }
                        else
                        {
                            swaps++;
                        }
                    } 
                    else if (nextSign != lastSign)
                    {
                        swaps++;
                    }
                    lastSign = nextSign;
                }
            }

                return swaps;
        }

        /// <summary>
        /// Returns x0 where f(x0) = 0 && for all x in range s.t. f(x) = 0, |x0| less than |x|
        /// Returns -1 if no root exists
        /// Requires: That f(range[0]) != 0 && f(range[1]) != 0
        /// Requires: range[0] less than range[1]
        /// Requires: range[0] > 0
        /// </summary>
        /// <param name="coefficients">coefficients on the polynomial of the form {A0,A1,...An}</param>
        /// <returns>Returns the smallest root of a polynomial
        /// Returns -1 if no root exists</returns>
        public static double findFirstRealRoot(double[] coefficients, double[] range)
        {
            double root = -1;
            double epsilon = 1e-4;
            //Check if there are roots to be found
            int numberOfRealRoots = findNumberOfRealRoots(coefficients, range);
            if (numberOfRealRoots == 0)
            {
                //If not just return
                return root;
            }

            //This part is an incomplete attempt to optimize the root finding by bisecting dynamically.
            //double L = Math.Abs(range[1] - range[0]);
            //const double t0 = 0.31;
            //int n = (int)Math.Log(L * t0 * Math.Log(2) / (7 * dx), 2) - 1;
            //while (n > 0)
            //{
            //    double[] newRange = { range[0], (range[1] + range[0]) / 2.0 };
            //    if (Math.Abs(polynomial(coefficients, newRange[1])) < epsilon)
            //    {
            //        return newRange[1];
            //    }
            //    numberOfRealRoots = findNumberOfRealRoots(coefficients, newRange);
            //    if (numberOfRealRoots == 0)
            //    {
            //        range[0] = newRange[1];
            //    }
            //    else
            //    {
            //        range[1] = newRange[1];
            //    }
            //    n--;
            //}

            //Now that we have isolated a single root, find it
            int f0 = Math.Sign(polynomial(coefficients, range[0]));
            double f = 1.0;
            double dx = (range[1] - range[0]) / 2.0;
            while (Math.Abs(f) > epsilon)
            {
                for (double x = range[0] + dx; x < range[1]; x += dx)
                {
                    f = polynomial(coefficients, x);
                    if (Math.Sign(f) != f0)
                    {
                        range[1] = x;
                        range[0] = x - dx;
                        break;
                    }
                }
                dx /= 5.0;
                root = (range[1] + range[0]) / 2.0;
                f = polynomial(coefficients, root);
            }
            return root;
        }


        #region Private Methods
        /// <summary>
        /// Finds the leading coefficient of a polynomial defined by
        /// poly = f[0] + f[1]x +... f[n]x^n
        /// </summary>
        /// <param name="a">Polynomial to find the leading coefficient of</param>
        /// <returns>The leading coefficient</returns>
        private static double LeadingCoefficient(double[] a)
        {
            int i = a.Length - 1;
            while (i >= 0)
            {
                if (Math.Abs(a[i]) > Double.Epsilon)
                {
                    return a[i];
                }
                i--;
            }
            return 0;
        }

        /// <summary>
        /// Reutrns the degree of polynomial a defined by poly = f[0] + f[1]x +... f[n]x^n
        /// </summary>
        /// <param name="a">Polynomial to find the degree of</param>
        /// <returns>The degree of the polynomial</returns>
        private static int Degree(double[] a)
        {
            int i = a.Length - 1;
            while (i >= 0)
            {
                if (Math.Abs(a[i]) > Double.Epsilon)
                {
                    return i;
                }
                i--;
            }
            return -1;
        }
        #endregion
    }
}
