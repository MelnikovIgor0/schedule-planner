using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Models;

namespace BLL.Services.Interfaces;

public interface ICsvParser
{
    InputData ParseDataFromCsv(string[] data);
}
