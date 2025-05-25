using System;
using System.Collections.Generic;

namespace TheMatch.Controllers
{
    public static class CompatibilityHelper
    {

        // Ключ — номер группы (шкалы), значение — (ID первой черты, ID второй черты, вес)
        public static readonly List<(byte traitA, byte traitB, double weight, string name)> TraitGroups = new()
        {
            (1, 2, 0.18, "Экстраверсия/Интроверсия"),
            (3, 4, 0.18, "Организованность/Спонтанность"),
            (5, 6, 0.16, "Эмоциональность/Сдержанность"),
            (7, 8, 0.16, "Новаторство/Привычность"),
            (9, 10, 0.16, "Лидерство/Сотрудничество"),
            (11, 12, 0.16, "Проявление внимания/Ожидание заботы")
        };

        // Основной метод с передачей traitConfigs
        public static double CalculateCompatibility(
            Dictionary<byte, decimal> userTraits,
            Dictionary<byte, decimal> otherTraits
        )
        {
            double totalWeight = 0;
            double weightedSum = 0;
            foreach (var (traitA, traitB, weight, name) in TraitGroups)
            {
                if (userTraits.TryGetValue(traitA, out var aA) && userTraits.TryGetValue(traitB, out var aB) &&
                    otherTraits.TryGetValue(traitA, out var bA) && otherTraits.TryGetValue(traitB, out var bB))
                {
                    var userValue = (double)(aA - aB) + 10;
                    var otherValue = (double)(bA - bB) + 10;
                    double similarity;
                    // Универсальное условие: оба выбрали "не знаю" дважды
                    if (aA == 5 && aB == 5 && bA == 5 && bB == 5)
                    {
                        // similarity = случайное значение от 0.70 до 0.80 с шагом 0.01
                        var rnd = new Random();
                        similarity = Math.Round(0.70 + rnd.Next(0, 11) * 0.01, 2);
                    }
                    else if (traitA == 9 && traitB == 10) // Лидерство/Сотрудничество
                    {
                        similarity = Math.Abs((double)aA - (double)bA) / 20.0;
                    }
                    else if (traitA == 11 && traitB == 12) // Проявление внимания/Ожидание заботы
                    {
                        // aA — проявление внимания пользователя, bA — проявление внимания анкеты
                        if (aA >= 17 && bA >= 17)
                            similarity = 1.0;
                        else if ((aA >= 17 && bA <= 10) || (aA <= 10 && bA >= 17))
                            similarity = 0.6;
                        else if (aA < 11 && bA < 11)
                            similarity = 0.3;
                        else
                            similarity = 0.5;
                    }
                    else
                    {
                        similarity = 1 - Math.Abs(userValue - otherValue) / 20.0;
                    }
                    weightedSum += similarity * weight;
                    totalWeight += weight;
                }
            }
            if (totalWeight == 0) return 0;
            return Math.Round(weightedSum / totalWeight * 100);
        }
    }
} 