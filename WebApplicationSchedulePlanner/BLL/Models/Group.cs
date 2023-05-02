using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Models;

public class Group
{
    public int Id;
    public string Name;
    public int NumberStudents;
    public List<Subject> Subjects;
}
