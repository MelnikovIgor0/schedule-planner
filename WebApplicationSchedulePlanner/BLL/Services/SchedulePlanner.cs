using BLL.Models;

namespace BLL.Services;

public sealed class SchedulePlanner : ISchedulePlanner
{
    /// <summary>
    /// Method gets all possible combinations of day, number lesson and subject
    /// which satisfy input data and sort them in specified order.
    /// </summary>
    /// <param name="data">Input data.</param>
    /// <returns>List of sorted combinations and precounted number of students visiting each subject.</returns>
    private List<(int day, int lesson, Subject subject, int numberStudents)> GetAllSlots(InputData data)
    {
        List<(int day, int lesson, Subject subject, int numberStudents)> slots = new();
        Dictionary<int, int> numberStudents = new();
        foreach (Student student in data.Students)
        {
            HashSet<int> subjectsIds = new HashSet<int>();
            foreach (Group group in student.Groups)
            {
                foreach (Subject subject in group.Subjects)
                {
                    subjectsIds.Add(subject.Id);
                }
            }
            foreach (int subjectId in subjectsIds)
            {
                if (numberStudents.ContainsKey(subjectId))
                {
                    numberStudents[subjectId]++;
                }
                else
                {
                    numberStudents[subjectId] = 1;
                }
            }
        }

        foreach (KeyValuePair<int, int> pair in numberStudents)
        {
            Console.WriteLine(pair.Key + ": " + pair.Value);
        }

        foreach (Subject subject in data.Subjects)
        {
            foreach (var slot in subject.Slots)
            {
                slots.Add((slot.day, slot.lesson, subject, numberStudents[subject.Id]));
            }
        }

        List<int> order = new();
        for (int i = 0; i < data.LessonPriorety.Count() + 1; ++i)
        {
            order.Add(0);
        }
        for (int i = 0; i < data.LessonPriorety.Count(); ++i)
        {
            order[data.LessonPriorety[i]] = i;
        }
        slots.Sort((a, b) =>
        {
            if (a.day == b.day)
            {
                if (a.lesson == b.lesson)
                {
                    return -(a.numberStudents.CompareTo(b.numberStudents));
                }
                return (order[a.lesson].CompareTo(order[b.lesson]));
            }
            return a.day.CompareTo(b.day);
        });

        return slots;
    }

    /// <summary>
    /// Method sets array of all possible combinations of
    /// day of week, lesson number and auditoria.
    /// </summary>
    /// <param name="data">Input data.</param>
    /// <returns>2D array of list of auditorias.</returns>
    private List<Auditorium>[,] GenerateAuditoriumsSlots(InputData data)
    {
        List<Auditorium>[,] auditorias = new List<Auditorium>[7, 9];
        for (int i = 0; i < auditorias.GetLength(0); ++i)
        {
            for (int j = 0; j < auditorias.GetLength(1); ++j)
            {
                auditorias[i, j] = new List<Auditorium>();
            }
        }
        for (int i = 0; i < auditorias.GetLength(0); ++i)
        {
            for (int j = 0; j < auditorias.GetLength(1); ++j)
            {
                foreach (Auditorium auditorium in data.Auditorias)
                {
                    auditorias[i, j].Add(auditorium);
                }
                auditorias[i, j].Sort((a, b) =>
                {
                    return a.Capacity.CompareTo(b.Capacity);
                });
            }
        }
        return auditorias;
    }

    /// <summary>
    /// Method crates list of hashsets, each hashset conclude ids of lessons 
    /// visited by each student.
    /// </summary>
    /// <param name="data">Input data.</param>
    /// <returns>List of hashets of ids of lessons.</returns>
    private List<HashSet<int>> GenerateLessonsForStudent(InputData data)
    {
        int sz = data.Students.Max(x => x.Id);
        List<HashSet<int>> answer = new();
        for (int i = 0; i < sz + 1; ++i)
        {
            answer.Add(new HashSet<int>());
        }
        foreach (Student student in data.Students)
        {
            foreach (Group group in student.Groups)
            {
                foreach (Subject subject in group.Subjects)
                {
                    answer[student.Id].Add(subject.Id);
                }
            }
        }
        return answer;
    }

    /// <summary>
    /// Method generated schedule by input data and priorety of lessons.
    /// </summary>
    /// <param name="data">Input data.</param>
    /// <returns>Timetable as list of elements.</returns>
    public List<ScheduleElement> GenerateSchedule(InputData data)
    {
        // Init all needed containers.
        List<(int day, int lesson, Subject subject, int numberStudents)> slots = 
            GetAllSlots(data);
        List<(int day, int lesson, Subject subject, int numberStudents)> skipped = new();
        List<Auditorium>[,] auditoriums = GenerateAuditoriumsSlots(data);
        List<ScheduleElement> answer = new();
        HashSet<int> placedLessons = new();
        HashSet<(int day, int lesson, int subjectId)> restrictions = new();
        List<HashSet<int>> lessonsForStudent = GenerateLessonsForStudent(data);
        int numberPlaced = 0;
        List<int> order = new();
        for (int i = 0; i < data.LessonPriorety.Count() + 1; ++i)
        {
            order.Add(0);
        }
        for (int i = 0; i < data.LessonPriorety.Count(); ++i)
        {
            order[data.LessonPriorety[i]] = i;
        }
        // Iterating by all possible slots and trying to place subject as soon as possible.
        foreach (var element in slots)
        {
            if (restrictions.Contains((element.day, element.lesson, element.subject.Id)))
            {
                continue;
            }
            if (placedLessons.Contains(element.subject.Id))
            {
                continue;
            }
            if (auditoriums[element.day, element.lesson].Count() == 0 ||
                auditoriums[element.day, element.lesson].Last().Capacity < element.numberStudents)
            {
                continue;
            }
            // Adding placed subject in generated timetable and updating information.
            answer.Add(new ScheduleElement
            {
                Id = ++numberPlaced,
                Lesson = element.lesson,
                AuditoriumName = auditoriums[element.day, element.lesson].Last().Name,
                Day = element.day,
                LessonName = element.subject.Name
            });
            auditoriums[element.day, element.lesson].RemoveAt(
                auditoriums[element.day, element.lesson].Count() - 1);
            placedLessons.Add(element.subject.Id);

            // Setting restrictions caused by placing this subject on current day and time.
            for (int i = 0; i < lessonsForStudent.Count(); ++i)
            {
                if (lessonsForStudent[i].Contains(element.subject.Id))
                {
                    foreach (int lessonId in lessonsForStudent[i])
                    {
                        restrictions.Add((element.day, element.lesson, lessonId));
                    }
                }
            }
        }

        // Finding subjects which were not placed in timetable.
        List<Subject> skippedSubjects = new();
        foreach (Subject subject in data.Subjects)
        {
            if (!placedLessons.Contains(subject.Id))
            {
                skippedSubjects.Add(subject);
            }
        }

        List<Subject> twiceSkipped = new();

        // Placing skipped subjects in most apropriate slots (in
        // all cases there will be overlap in schedule).
        foreach (Subject subject in skippedSubjects)
        {
            bool placed = false;
            int numberStudents = -1;
            foreach (var slot in slots)
            {
                if (slot.subject.Id == subject.Id)
                {
                    numberStudents = slot.numberStudents;
                    break;
                }
            }

            subject.Slots.Sort((a, b) =>
            {
                if (a.day == b.day)
                {
                    return a.lesson.CompareTo(b.lesson);
                }
                return a.day.CompareTo(b.day);
            });

            subject.Slots.Sort((a, b) =>
            {
                if (auditoriums[a.day, a.lesson].Count() == auditoriums[b.day, b.lesson].Count())
                {
                    return (order[a.lesson].CompareTo(order[b.lesson]));
                }
                return -(auditoriums[a.day, a.lesson].Count().CompareTo(auditoriums[b.day, b.lesson].Count()));
            });

            foreach (var slot in subject.Slots)
            {
                if (auditoriums[slot.day, slot.lesson].Count() > 0 && 
                    auditoriums[slot.day, slot.lesson].Last().Capacity >= numberStudents)
                {
                    answer.Add(new ScheduleElement
                    {
                        Id = ++numberPlaced,
                        Lesson = slot.lesson,
                        AuditoriumName = auditoriums[slot.day, slot.lesson].Last().Name,
                        Day = slot.day,
                        LessonName = subject.Name
                    });
                    auditoriums[slot.day, slot.lesson]
                        .RemoveAt(auditoriums[slot.day, slot.lesson].Count() - 1);
                    placed = true;
                    break;
                }
            }
            if (!placed)
            {
                twiceSkipped.Add(subject);
            }
        }

        int[,] numberLessons = new int[7, 9];
        foreach (ScheduleElement element in answer)
        {
            ++numberLessons[element.Day, element.Lesson];
        }

        // Pushing twice skipped subjects to online :(
        foreach (Subject subject in twiceSkipped)
        {
            subject.Slots.Sort((a, b) =>
            {
                if (numberLessons[a.day, a.lesson] == numberLessons[b.day, b.lesson])
                {
                    return (order[a.lesson].CompareTo(order[b.lesson]));
                }
                return numberLessons[a.day, a.lesson].CompareTo(numberLessons[b.day, b.lesson]);
            });
            answer.Add(new ScheduleElement
            {
                Id = ++numberPlaced,
                Lesson = subject.Slots[0].lesson,
                Day = subject.Slots[0].day,
                AuditoriumName = "ONLINE",
                LessonName = subject.Name
            });
            ++numberLessons[subject.Slots[0].day, subject.Slots[0].lesson];
        }

        // Sorting placed now subjects.
        answer.Sort((a, b) =>
        {
            if (a.Day == b.Day)
            {
                return a.Lesson.CompareTo(b.Lesson);
            }
            return a.Day.CompareTo(b.Day);
        });

        return answer;
    }
}
