using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Numerics;
using System.Text.RegularExpressions;

namespace ApiProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TextController : ControllerBase
    {
        public class InputModel
        {
            public string UserText { get; set; }
        }

        private readonly string[] words = new string[]
         {
            "sıfır", "bir", "iki", "üç", "dört", "beş", "altı", "yedi", "sekiz", "dokuz",
            "on", "yirmi", "otuz", "kırk", "elli", "altmış", "yetmiş", "seksen", "doksan",
            "yüz", "bin", "milyon", "milyar", "trilyon"
         };

        private readonly BigInteger[] values = new BigInteger[]
        {
            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
            1000, 1000000, 1000000000, 1000000000000
        };


        // POST api/text
        [HttpPost]
        public IActionResult Post([FromBody] InputModel inputModel)
        {
            if (inputModel == null)
            {
                return BadRequest("Geçerli bir metin gönderilmedi.");
            }
            else
            {
                try
                {
                    var userText = inputModel.UserText;
                    var outputText = ConvertTextToNumber(userText);
                    var outputObj = new { Output = outputText };
                    var outputJson = JsonConvert.SerializeObject(outputObj);

                    return Ok(outputJson);
                }
                catch (Exception ex)
                {
                    return BadRequest("Çevirme işlemi sırasında hata oluştu! Lütfen kontrol ediniz!!!" + ex.Message);
                }
                
            }
            
        }
        private List<string> SplitCombinedNumbers(string text)
        {
            text = ConvertToLowerCaseAndTurkish(text);
            text = Regex.Replace(text, @"(\D)(\d)", "$1 $2");
            foreach (var word in words)
            {
                if (text.Contains(word))
                {
                    text = text.Replace(word, $" {word} ");
                }
            }
            return text.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        }

        private string ConvertToLowerCaseAndTurkish(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }
            string lowerCaseText = text.ToLower();// Küçük harfe dönüştür

            //lowerCaseText = lowerCaseText.Replace("i", "ı")
            //                             .Replace("ü", "u")
            //                             .Replace("ö", "o")
            //                             .Replace("ş", "s")
            //                             .Replace("ç", "c");// Türkçe karakterleri çevir

            //lowerCaseText = lowerCaseText.Replace("ı", "i")
            //                             .Replace("u", "ü")
            //                             .Replace("o", "ö")
            //                             .Replace("s", "ş")
            //                             .Replace("c", "ç");// Türkçe karakterleri çevir
            return lowerCaseText;

        }

        List<String> NewNumberList = new List<string>();
        private string ConvertTextToNumber(string text)
        {
            List<String> tokensList = SplitCombinedNumbers(text);
            BigInteger _result = 0;
            BigInteger _current = 0;
            BigInteger _partValue = 0;
            string ResultPart = "";

            do
            {
                maxValue = 0;
                NewNumberList = Calculate(tokensList.ToList());
                if (NewNumberList.Count == 0 && tokensList.Count > 0)
                {
                    string _str = tokensList[0];
                    string _part = _partValue == 0 ? "" :_partValue.ToString();
                    ResultPart += _part + " " + _str + " ";
                    ResultPart = Regex.Replace(ResultPart, @"\s+", " ");
                    _partValue = 0;
                    tokensList.RemoveAt(0);
                }
                for (int i = 0; i < NewNumberList.Count; i++)
                {
                    tokensList.RemoveAt(0);
                    string token = NewNumberList[i];

                    int count = 0;
                    int index = Array.IndexOf(words, token);

                    BigInteger value;

                    if (index < 0)
                    {
                        BigInteger.TryParse(token, out value);
                    }
                    else
                    {
                        value = values[index];
                    }

                    if (value == 100)
                    {
                        _current = value;
                        _result = CalculateResult(_result, _current);
                    }
                    else if (value >= 1000)
                    {
                        _current = value;
                        _result = CalculateResult(_result, _current);
                    }
                    else
                    {
                        _current = value;
                        _result = CalculateResult(_result, _current);

                    }

                    if (_current == maxValue)
                    {
                        _partValue += _result;
                        _current = 0;
                        _result = 0;
                    }
                    count++;
                }

            } while (tokensList.Count > 0);

            return ResultPart.Trim() + (_partValue == 0 ? "" : " "+_partValue.ToString()) ;
        }

        private BigInteger CalculateResult(BigInteger result, BigInteger current)
        {
            if (result == 0)
            {
                return current;
            }
            else
            {
                if (current > result)
                {
                    return result * current;
                }
                else
                {
                    return result + current;
                }
            }
        }

        BigInteger maxValue = 0;
        private List<String> Calculate(List<String> tokens)
        {
            int count = 0;
            
            foreach (var number in tokens)
            {
                BigInteger _value;
                int _indexValue = Array.IndexOf(words.ToArray(), number);

                if (_indexValue < 0)
                {
                    BigInteger num = -1;
                    if (!BigInteger.TryParse(number, out num))
                    {
                        return tokens.GetRange(0, count);
                    }
                    int _newIndexValue = Array.IndexOf(values.ToArray(), num);

                    if (_newIndexValue > 0)
                        _value = values[_newIndexValue];

                    if (BigInteger.TryParse(number, out _value))
                    {
                        _value = BigInteger.Parse(number);
                    }
                }

                else
                    _value = values[_indexValue];

                if (_value > maxValue)
                {
                    maxValue = _value;
                    count = Array.IndexOf(tokens.ToArray(), number) + 1;
                }
            }
            return tokens.GetRange(0, count);
        }
    }
}