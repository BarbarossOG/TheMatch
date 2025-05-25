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
            foreach (var (traitA, traitB, weight, _) in TraitGroups)
            {
                if (userTraits.TryGetValue(traitA, out var aA) && userTraits.TryGetValue(traitB, out var aB) &&
                    otherTraits.TryGetValue(traitA, out var bA) && otherTraits.TryGetValue(traitB, out var bB))
                {
                    // Преобразуем пару в одну шкалу: (A - B) + 10 (чтобы диапазон был 0-20)
                    var userValue = (double)(aA - aB) + 10;
                    var otherValue = (double)(bA - bB) + 10;
                    double similarity = 1 - Math.Abs(userValue - otherValue) / 20.0;
                    weightedSum += similarity * weight;
                    totalWeight += weight;
                }
            }
            if (totalWeight == 0) return 0;
            return Math.Round(weightedSum / totalWeight * 100);
        }
    }
} 