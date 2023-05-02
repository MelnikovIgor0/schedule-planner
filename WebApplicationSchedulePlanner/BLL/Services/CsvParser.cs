using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BLL.Services.Interfaces;
using BLL.Models;

namespace BLL.Services;

public sealed class CsvParser : ICsvParser
{
    private (int, int) ParseSlot(string data)
    {
        if (data.Length != 3)
        {
            throw new ArgumentException("Wrong format of slot for lesson");
        }
        int numberLesson;
        if (!int.TryParse(data[2].ToString(), out numberLesson))
        {
            throw new ArgumentException("Wrong value of lesson number");
        }
        if (data.StartsWith("mo"))
        {
            return (0, numberLesson);
        }
        if (data.StartsWith("tu"))
        {
            return (1, numberLesson);
        }
        if (data.StartsWith("we"))
        {
            return (2, numberLesson);
        }
        if (data.StartsWith("th"))
        {
            return (3, numberLesson);
        }
        if (data.StartsWith("fr"))
        {
            return (4, numberLesson);
        }
        if (data.StartsWith("sa"))
        {
            return (5, numberLesson);
        }
        if (data.StartsWith("su"))
        {
            return (6, numberLesson);
        }
        throw new ArgumentException("Wrong format of day in slot description");
    }

    private Subject ParseSubject(string line, int id)
    {
        string[] data = line.Split(';');
        if (data.Length < 2)
        {
            throw new ArgumentException("Wrong format of lesson");
        }
        int numberSlots;
        if (!int.TryParse(data[1], out numberSlots))
        {
            throw new ArgumentException("Wrong number of slots for lesson");
        }
        if (numberSlots < 1)
        {
            throw new ArgumentException("Wrong number of slots for lesson");
        }
        if (data.Length < 2 + numberSlots)
        {
            throw new ArgumentException("Wrong format of specifing number of slots");
        }
        Subject result = new Subject()
        {
            Name = data[0],
            Id = id,
            Slots = new()
        };
        for (int i = 0; i < numberSlots; ++i)
        {
            result.Slots.Add(ParseSlot(data[2 + i]));
        }
        if (result.Slots.Count() != result.Slots.Distinct().Count())
        {
            throw new ArgumentException("Found repetiotions in slots for lesson");
        }
        return result;
    }

    private Subject ParseSubjects(string line, InputData data)
    {
        foreach (Subject s in data.Subjects)
        {
            if (s.Name == line)
            {
                return s;
            }
        }
        throw new ArgumentException("Wrong subject specified for group");
    }

    private Group ParseGroup(string line, int id, InputData inputData)
    {
        string[] data = line.Split(';');
        if (data.Length < 2)
        {
            throw new ArgumentException("Wrong format of group description");
        }
        int numberLessons;
        if (!int.TryParse(data[1], out numberLessons))
        {
            throw new ArgumentException("Wrong number of lessons for group");
        }
        if (numberLessons < 1 || numberLessons > 10)
        {
            throw new ArgumentException("Wrong number of lessons for group");
        }
        if (data.Length < 2 + numberLessons)
        {
            throw new ArgumentException("Wrong format of specifiing lessons for group");
        }
        Group group = new Group()
        {
            Name = data[0],
            Id = id,
            NumberStudents = 0,
            Subjects = new()
        };
        for (int i = 0; i < numberLessons; ++i)
        {
            group.Subjects.Add(ParseSubjects(data[2 + i], inputData));
        }
        if (group.Subjects.Count() != group.Subjects.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in subjects for group");
        }
        return group;
    }

    private Group ParseGroup(string name, InputData data)
    {
        foreach (Group group in data.Groups)
        {
            if (group.Name == name)
            {
                return group;
            }
        }
        throw new ArgumentException("Wrong group name specified for student");
    }

    private Student ParseStudent(string line, int id, InputData inputData)
    {
        string[] data = line.Split(';');
        if (data.Length < 2)
        {
            throw new ArgumentException("Wrong format of description of student");
        }
        int numberGroups;
        if (!int.TryParse(data[1], out numberGroups))
        {
            throw new ArgumentException("Wrong number of groups for student");
        }
        if (numberGroups < 0 || numberGroups > 5)
        {
            throw new ArgumentException("Wrong number of groups for student");
        }
        if (data.Length < 2 + numberGroups)
        {
            throw new ArgumentException("Wrong format of specifing groups for student");
        }
        Student student = new Student()
        {
            Name = data[0],
            Id = id,
            Groups = new()
        };
        for (int i = 0; i < numberGroups; ++i)
        {
            student.Groups.Add(ParseGroup(data[2 + i], inputData));
        }
        foreach (Group group in student.Groups)
        {
            ++group.NumberStudents;
        }
        if (student.Groups.Count() != student.Groups.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in groups in description of students");
        }
        return student;
    }

    private Auditorium ParseAuditorium(string line, int id)
    {
        string[] data = line.Split(';');
        if (data.Length < 2)
        {
            throw new ArgumentException("Wrong format of description of auditorium");
        }
        int capacity;
        if (!int.TryParse(data[1], out capacity))
        {
            throw new ArgumentException("Wrong value of capacity of auditorium");
        }
        if (capacity < 1 || capacity > 300)
        {
            throw new ArgumentException("Wrong value of capacity of auditorium");
        }
        return new Auditorium()
        {
            Name = data[0],
            Id = id,
            Capacity = capacity
        };
    }

    public InputData ParseDataFromCsv(string[] data)
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Empty file");
        }
        InputData result = new InputData()
        {
            Auditorias = new(),
            Groups = new(),
            Students = new(),
            Subjects = new()
        };
        string[] dataLengths = data[0].Split(';');
        if (dataLengths.Length < 4)
        {
            throw new ArgumentException("Wrong format of row with sizes of data");
        }
        int[] sizes = new int[4];
        for (int i = 0; i < 4; ++i)
        {
            if (!int.TryParse(dataLengths[i], out sizes[i]))
            {
                throw new ArgumentException("Wrong value of size of one of parameters");
            }
        }
        if (sizes[0] < 1 || sizes[0] > 100)
        {
            throw new ArgumentException("Wrong number of subjects");
        }
        if (sizes[1] < 1 || sizes[1] > 30)
        {
            throw new ArgumentException("Wrong number of study groups");
        }
        if (sizes[2] < 1 || sizes[2] > 500)
        {
            throw new ArgumentException("Wrong number of students");
        }
        if (sizes[3] < 1 || sizes[3] > 50)
        {
            throw new ArgumentException("Wrong number of auditories");
        }
        if (data.Length != 1 + sizes[0] + sizes[1] + sizes[2] + sizes[3])
        {
            throw new ArgumentException("Wrong number of lines in input file");
        }
        int pos = 1;

        for (int i = 0; i < sizes[0]; ++i)
        {
            result.Subjects.Add(ParseSubject(data[pos], i + 1));
            ++pos;
        }
        IEnumerable<string> subjectNames = result.Subjects.Select(x => x.Name);
        if (subjectNames.Count() != subjectNames.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in subject names");
        }

        for (int i = 0; i < sizes[1]; ++i)
        {
            result.Groups.Add(ParseGroup(data[pos], i + 1, result));
            ++pos;
        }
        IEnumerable<string> groupsNames = result.Groups.Select(x => x.Name);
        if (groupsNames.Count() != groupsNames.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in groups names");
        }

        for (int i = 0; i < sizes[2]; ++i)
        {
            result.Students.Add(ParseStudent(data[pos], i + 1, result));
            ++pos;
        }
        IEnumerable<string> studentsNames = result.Students.Select(x => x.Name);
        if (studentsNames.Count() != studentsNames.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in students names");
        }

        for (int i = 0; i < sizes[3]; ++i)
        {
            result.Auditorias.Add(ParseAuditorium(data[pos], i + 1));
            ++pos;
        }
        IEnumerable<string> auditoriasNames = result.Auditorias.Select(x => x.Name);
        if (auditoriasNames.Count() != auditoriasNames.Distinct().Count())
        {
            throw new ArgumentException("Found repetitions in auditorias names");
        }

        return result;
    }
}
