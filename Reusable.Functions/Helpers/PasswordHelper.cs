using System.Security.Cryptography;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices.WindowsRuntime;

public class PasswordHelper
{
    public static string AllLowerCaseChars { get; private set; }
    public static string AllUpperCaseChars { get; private set; }
    public static string AllNumericChars { get; private set; }
    public static string AllSpecialChars { get; private set; }

    private readonly string _allAvailableChars;

    private readonly RandomSecureVersion _randomSecure = new RandomSecureVersion();
    private int _minimumNumberOfChars;

    static PasswordHelper()
    {
        // Define characters that are valid and reject ambiguous characters such as ilo, IO and 1 or 0
        AllLowerCaseChars = GetCharRange('a', 'z', exclusiveChars: "ilo");
        AllUpperCaseChars = GetCharRange('A', 'Z', exclusiveChars: "IO");
        AllNumericChars = GetCharRange('2', '9');
        AllSpecialChars = "!@#%*()$?+-=";
    }

    public PasswordHelper()
    {
        int minimumLowerCaseChars = 4;
        int minimumUpperCaseChars = 4;
        int minimumNumericChars = 4;
        int minimumSpecialChars = 6;

        _minimumNumberOfChars = minimumLowerCaseChars + minimumUpperCaseChars +
                                minimumNumericChars + minimumSpecialChars;

        _allAvailableChars =
            OnlyIfOneCharIsRequired(minimumLowerCaseChars, AllLowerCaseChars) +
            OnlyIfOneCharIsRequired(minimumUpperCaseChars, AllUpperCaseChars) +
            OnlyIfOneCharIsRequired(minimumNumericChars, AllNumericChars) +
            OnlyIfOneCharIsRequired(minimumSpecialChars, AllSpecialChars);
    }

    private string OnlyIfOneCharIsRequired(int minimum, string allChars)
    {
        return minimum > 0 || _minimumNumberOfChars == 0 ? allChars : string.Empty;
    }

    /// <summary>
    /// To generate random password
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    public string Generate(ILogger log)
    {
        try
        {
            log.LogInformation("Entered into Generate method");
            int lengthOfPassword = 32;
            int minimumLowerCaseChars = 2, minimumUpperCaseChars = 2, minimumNumericChars = 4, minimumSpecialChars = 4;

            // Get the required number of characters of each catagory and 
            // add random charactes of all catagories
            string minimumChars = GetRandomString(AllLowerCaseChars, minimumLowerCaseChars) +
                            GetRandomString(AllUpperCaseChars, minimumUpperCaseChars) +
                            GetRandomString(AllNumericChars, minimumNumericChars) +
                            GetRandomString(AllSpecialChars, minimumSpecialChars);
            log.LogInformation("Generated random string for password");

            string unshuffeledResult = String.Concat(minimumChars, GetRandomString(_allAvailableChars, lengthOfPassword - minimumChars.Length));

            // Shuffle the result so the order of the characters are unpredictable
            return unshuffeledResult.ShuffleTextSecure();
        }
        catch (Exception ex)
        {
            log.LogInformation($"\n CreateBearerToken got Exception \n Time: { DateTime.Now} \n Exception{ ex.Message}");
            return string.Empty;
        }
        
    }

    /// <summary>
    /// To generate a random string for given characters and length
    /// </summary>
    /// <param name="possibleChars"></param>
    /// <param name="lenght"></param>
    /// <returns></returns>
    private string GetRandomString(string possibleChars, int lenght)
    {
        string result = string.Empty;
        for (int position = 0; position < lenght; position++)
        {
            int index = _randomSecure.Next(possibleChars.Length);
            result += possibleChars[index];
        }
        return result;
    }

    /// <summary>
    /// To get characters range for given inputs
    /// </summary>
    /// <param name="minimum"></param>
    /// <param name="maximum"></param>
    /// <param name="exclusiveChars"></param>
    /// <returns></returns>
    private static string GetCharRange(char minimum, char maximum, string exclusiveChars = "")
    {
        string result = string.Empty;
        for (char value = minimum; value <= maximum; value++)
        {
            result += value;
        }
        if (!string.IsNullOrEmpty(exclusiveChars))
        {
            var inclusiveChars = result.Except(exclusiveChars).ToArray();
            result = new string(inclusiveChars);
        }
        return result;
    }
}

internal static class Extensions
{
    public static string ShuffleTextSecure(this string source)
    {
        var shuffeldChars = source.ShuffleSecure().ToArray();
        return new string(shuffeldChars);
    }
    private static readonly Lazy<RandomSecureVersion> RandomSecure =
        new Lazy<RandomSecureVersion>(() => new RandomSecureVersion());
    public static IEnumerable<T> ShuffleSecure<T>(this IEnumerable<T> source)
    {
        var sourceArray = source.ToArray();
        for (int counter = 0; counter < sourceArray.Length; counter++)
        {
            int randomIndex = RandomSecure.Value.Next(counter, sourceArray.Length);
            yield return sourceArray[randomIndex];

            sourceArray[randomIndex] = sourceArray[counter];
        }
    }
}

internal class RandomSecureVersion
{
    private readonly RNGCryptoServiceProvider _rngProvider = new RNGCryptoServiceProvider();

    public int Next()
    {
        var randomBuffer = new byte[4];
        _rngProvider.GetBytes(randomBuffer);
        int result = BitConverter.ToInt32(randomBuffer, 0);
        return result;
    }

    public int Next(int maximumValue)
    {
        // Do not use Next() % maximumValue because the distribution is not OK
        return Next(0, maximumValue);
    }

    public int Next(int minimumValue, int maximumValue)
    {
        var seed = Next();
        //  Generate uniformly distributed random integers within a given range.
        return new Random(seed).Next(minimumValue, maximumValue);
    }
}