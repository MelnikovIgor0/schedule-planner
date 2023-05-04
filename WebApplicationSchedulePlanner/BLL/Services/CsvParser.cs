using BLL.Services.Interfaces;
using BLL.Models;

namespace BLL.Services;

public sealed class CsvParser : ICsvParser
{
    private (int, int) ParseSlot(string data, int lineNumber)
    {
        if (data.Length != 3)
        {
            throw new ArgumentException($"Неправильый формат указания слота для пары (строка {lineNumber})");
        }
        int numberLesson;
        if (!int.TryParse(data[2].ToString(), out numberLesson))
        {
            throw new ArgumentException($"Несоответствие указанного и " +
                $"фактичесого количества слотов для пары (строка {lineNumber})");
        }
        if (data.StartsWith("пн"))
        {
            return (0, numberLesson);
        }
        if (data.StartsWith("вт"))
        {
            return (1, numberLesson);
        }
        if (data.StartsWith("ср"))
        {
            return (2, numberLesson);
        }
        if (data.StartsWith("чт"))
        {
            return (3, numberLesson);
        }
        if (data.StartsWith("пт"))
        {
            return (4, numberLesson);
        }
        if (data.StartsWith("сб"))
        {
            return (5, numberLesson);
        }
        if (data.StartsWith("вс"))
        {
            return (6, numberLesson);
        }
        throw new ArgumentException($"Неправильный формат слота (строка {lineNumber})");
    }

    private Subject ParseSubject(string line, int id, int limit, int lineNumber)
    {
        string[] data = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 2)
        {
            throw new ArgumentException($"Неправильный формат указания предмета (строка {lineNumber})");
        }
        int numberSlots;
        if (!int.TryParse(data[1], out numberSlots))
        {
            throw new ArgumentException($"Неправильно указано " +
                $"количество слотов для предмета (строка {lineNumber})");
        }
        if (numberSlots < 1)
        {
            throw new ArgumentException($"Некорректное количество " +
                $"слотов для предмета (строка {lineNumber})");
        }
        if (data.Length != 2 + numberSlots)
        {
            throw new ArgumentException($"Указанное количество слотов" +
                $" для предмета не соответствует фактическому " +
                $"количеству слотов (строка {lineNumber})");
        }
        Subject result = new Subject()
        {
            Name = data[0],
            Id = id,
            Slots = new()
        };
        for (int i = 0; i < numberSlots; ++i)
        {
            result.Slots.Add(ParseSlot(data[2 + i], lineNumber));
            if (result.Slots.Last().lesson > limit)
            {
                throw new ArgumentException($"Указан слот, номер пары в котором" +
                    $" превышает максимальный номер пары (строка {lineNumber})");
            }
        }
        if (result.Slots.Count() != result.Slots.Distinct().Count())
        {
            throw new ArgumentException($"Обнаружены повторы в слотах для одного предмета (строка {lineNumber})");
        }
        return result;
    }

    private Subject ParseSubjects(string line, InputData data, int lineNumber)
    {
        foreach (Subject s in data.Subjects)
        {
            if (s.Name == line)
            {
                return s;
            }
        }
        throw new ArgumentException($"В описании группы найден неописанный предмет (строка {lineNumber})");
    }

    private Group ParseGroup(string line, int id, InputData inputData, int lineNumber)
    {
        string[] data = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 2)
        {
            throw new ArgumentException($"Неправильный формат описания группы (строка {lineNumber})");
        }
        int numberLessons;
        if (!int.TryParse(data[1], out numberLessons))
        {
            throw new ArgumentException($"Неправильно указано количество " +
                $"предметов в группе (строка {lineNumber})");
        }
        if (numberLessons < 1 || numberLessons > 100)
        {
            throw new ArgumentException($"Неправильно указано количество " +
                $"предметов в группе (строка {lineNumber})");
        }
        if (data.Length != 2 + numberLessons)
        {
            throw new ArgumentException($"Указанное количество предметов в " +
                $"группе не соответствует фактическому количеству " +
                $"предметов (строка {lineNumber})");
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
            group.Subjects.Add(ParseSubjects(data[2 + i], inputData, lineNumber));
        }
        if (group.Subjects.Count() != group.Subjects.Distinct().Count())
        {
            throw new ArgumentException($"Найдены повторы в описании предметов группы");
        }
        return group;
    }

    private Group ParseGroup(string name, InputData data, int lineNumber)
    {
        foreach (Group group in data.Groups)
        {
            if (group.Name == name)
            {
                return group;
            }
        }
        throw new ArgumentException($"При описании студента указана" +
            $" неописанная группа (строка {lineNumber})");
    }

    private Student ParseStudent(string line, int id, InputData inputData, int lineNumber)
    {
        string[] data = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length < 2)
        {
            throw new ArgumentException($"Неправильный формат описания студента (строка {lineNumber})");
        }
        int numberGroups;
        if (!int.TryParse(data[1], out numberGroups))
        {
            throw new ArgumentException($"Указано некорректное количество " +
                $"групп, в которые входит студент (строка {lineNumber})");
        }
        if (numberGroups < 0 || numberGroups > 5)
        {
            throw new ArgumentException($"Указано некорректное количество " +
                $"групп, в которые входит студент (строка {lineNumber})");
        }
        if (data.Length != 2 + numberGroups)
        {
            throw new ArgumentException($"Указанное количество групп для студента" +
                $" не соответствует фактическому количеству" +
                $" перечисленных групп (строка {lineNumber})");
        }
        Student student = new Student()
        {
            Name = data[0],
            Id = id,
            Groups = new()
        };
        for (int i = 0; i < numberGroups; ++i)
        {
            student.Groups.Add(ParseGroup(data[2 + i], inputData, lineNumber));
        }
        foreach (Group group in student.Groups)
        {
            ++group.NumberStudents;
        }
        if (student.Groups.Count() != student.Groups.Distinct().Count())
        {
            throw new ArgumentException("Найдены повторы при описании групп, в которые входит студент");
        }
        return student;
    }

    private Auditorium ParseAuditorium(string line, int id, int lineNumber)
    {
        string[] data = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length != 2)
        {
            throw new ArgumentException($"Неправильный формат описания аудитории (строка {lineNumber})");
        }
        int capacity;
        if (!int.TryParse(data[1], out capacity))
        {
            throw new ArgumentException($"Неправильно указана вместимость аудитории (строка {lineNumber})");
        }
        if (capacity < 1 || capacity > 500)
        {
            throw new ArgumentException($"Неправильно указана вместимость аудитории (строка {lineNumber})");
        }
        return new Auditorium()
        {
            Name = data[0],
            Id = id,
            Capacity = capacity
        };
    }

    private List<int> ParseLessonPriorety(int numberLessons, string line)
    {
        string[] data = line.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (data.Length != numberLessons)
        {
            throw new ArgumentException("Неправильно указаны номера приоритетных пар (строка 2)");
        }
        int[] lessons = new int[numberLessons];
        for (int i = 0; i < lessons.Length; ++i)
        {
            if (!int.TryParse(data[i], out lessons[i]))
            {
                throw new ArgumentException("Неправильно " +
                    "указаны номера приоритетных пар (строка 2)");
            }
        }
        List<int> values1 = new();
        List<int> values2 = new();
        for (int i = 0; i < numberLessons; ++i)
        {
            values1.Add(i + 1);
            values2.Add(lessons[i]);
        }
        values2.Sort();
        for (int i = 0; i < numberLessons; ++i)
        {
            if (values1[i] != values2[i])
            {
                throw new ArgumentException("Указанные приоритетные пары " +
                    "не образуют корректную перестановку (строка 2)");
            }
        }
        return new List<int>(lessons);
    }

    public InputData ParseDataFromCsv(string[] data)
    {
        if (data.Length == 0)
        {
            throw new ArgumentException("Загружен пустой файл");
        }
        InputData result = new InputData()
        {
            Auditorias = new(),
            Groups = new(),
            Students = new(),
            Subjects = new(),
            LessonPriorety = new()
        };
        string[] dataLengths = data[0].Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (dataLengths.Length != 5)
        {
            throw new ArgumentException("Неправильно указаны данные о количествах объектов (строка 1)");
        }
        int[] sizes = new int[5];
        for (int i = 0; i < 5; ++i)
        {
            if (!int.TryParse(dataLengths[i], out sizes[i]))
            {
                throw new ArgumentException("Неправильно указан один из параметров (строка 1)");
            }
        }
        if (sizes[0] < 1 || sizes[0] > 100)
        {
            throw new ArgumentException("Некорректное количество предметов (строка 1)");
        }
        if (sizes[1] < 1 || sizes[1] > 30)
        {
            throw new ArgumentException("Некорректное количество групп (строка 1)");
        }
        if (sizes[2] < 1 || sizes[2] > 500)
        {
            throw new ArgumentException("Некорректное количество студентов (строка 1)");
        }
        if (sizes[3] < 1 || sizes[3] > 50)
        {
            throw new ArgumentException("Некорректное количество аудиторий (строка 1)");
        }
        if (sizes[4] < 1 || sizes[4] > 8)
        {
            throw new ArgumentException("Некорректное значение максимального количества пар в день (строка 1)");
        }
        if (data.Length != 2 + sizes[0] + sizes[1] + sizes[2] + sizes[3])
        {
            throw new ArgumentException("Некорректное количество строк в файле");
        }
        result.LessonPriorety = ParseLessonPriorety(sizes[4], data[1]);
        int pos = 2;

        for (int i = 0; i < sizes[0]; ++i)
        {
            result.Subjects.Add(ParseSubject(data[pos], i + 1, result.LessonPriorety.Count(), pos + 1));
            ++pos;
        }
        IEnumerable<string> subjectNames = result.Subjects.Select(x => x.Name);
        if (subjectNames.Count() != subjectNames.Distinct().Count())
        {
            throw new ArgumentException("Найдены повторения в названиях предметов");
        }

        for (int i = 0; i < sizes[1]; ++i)
        {
            result.Groups.Add(ParseGroup(data[pos], i + 1, result, pos + 1));
            ++pos;
        }
        IEnumerable<string> groupsNames = result.Groups.Select(x => x.Name);
        if (groupsNames.Count() != groupsNames.Distinct().Count())
        {
            throw new ArgumentException("Найдены повторения в названиях групп");
        }

        for (int i = 0; i < sizes[2]; ++i)
        {
            result.Students.Add(ParseStudent(data[pos], i + 1, result, pos + 1));
            ++pos;
        }
        IEnumerable<string> studentsNames = result.Students.Select(x => x.Name);
        if (studentsNames.Count() != studentsNames.Distinct().Count())
        {
            throw new ArgumentException("Найдены повторения в именах студентов");
        }

        for (int i = 0; i < sizes[3]; ++i)
        {
            result.Auditorias.Add(ParseAuditorium(data[pos], i + 1, pos + 1));
            ++pos;
        }
        IEnumerable<string> auditoriasNames = result.Auditorias.Select(x => x.Name);
        if (auditoriasNames.Count() != auditoriasNames.Distinct().Count())
        {
            throw new ArgumentException("Найдены повторения в названиях аудиторий");
        }

        return result;
    }
}
