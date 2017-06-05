﻿/*
 *Ваши права на использования кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OsEngine.Entity;

namespace OsEngine.Charts.CandleChart.Indicators
{

    /// <summary>
    /// индикатор ATR. Average True Range
    /// </summary>
    public class Atr : IIndicatorCandle
    {

        /// <summary>
        /// конструктор с параметрами. Индикатор будет сохраняться
        /// </summary>
        /// <param name="uniqName">уникальное имя</param>
        /// <param name="canDelete">можно ли пользователю удалить индикатор с графика вручную</param>
        public Atr(string uniqName, bool canDelete)
        {
            Name = uniqName;
            Lenght = 14;
            TypeIndicator = IndicatorOneCandleChartType.Line;
            TypeCalculationAverage = MovingAverageTypeCalculation.Simple;
            ColorBase = Color.DodgerBlue;
            PaintOn = true;
            CanDelete = canDelete;
            Load();
        }

        /// <summary>
        /// конструктор без параметров. Индикатор не будет сохраняться
        /// используется ТОЛЬКО для создания составных индикаторов
        /// не используйте его из слоя создания роботов!
        /// </summary>
        /// <param name="canDelete">можно ли пользователю удалить индикатор с графика вручную</param>
        public Atr(bool canDelete)
        {
            Name = Guid.NewGuid().ToString();

            Lenght = 14;
            TypeIndicator = IndicatorOneCandleChartType.Line;
            TypeCalculationAverage = MovingAverageTypeCalculation.Simple;
            ColorBase = Color.DodgerBlue;
            PaintOn = true;
            CanDelete = canDelete;
        }

        /// <summary>
        /// все значения индикатора
        /// </summary>
        List<List<decimal>> IIndicatorCandle.ValuesToChart
        {
            get
            {
                List<List<decimal>> list = new List<List<decimal>>();
                list.Add(Values);
                return list;
            }
        }

        /// <summary>
        /// цвета для индикатора
        /// </summary>
        List<Color> IIndicatorCandle.Colors
        {
            get
            {
                List<Color> colors = new List<Color>();
                colors.Add(ColorBase);
                return colors;
            }

        }

        /// <summary>
        /// можно ли удалить индикатор с графика. Это нужно для того чтобы у роботов нельзя было удалить 
        /// индикаторы которые ему нужны в торговле
        /// </summary>
        public bool CanDelete { get; set; }

        /// <summary>
        /// тип прорисовки индикатора
        /// </summary>
        public IndicatorOneCandleChartType TypeIndicator { get; set; }

        /// <summary>
        /// тип скользящей средней для рассчёта индикатора
        /// </summary>
        public MovingAverageTypeCalculation TypeCalculationAverage;

        /// <summary>
        /// имя серии данных на которой будет прорисован индикатор
        /// </summary>
        public string NameSeries { get; set; }

        /// <summary>
        /// имя области данных на которой будет прорисовываться индикатор
        /// </summary>
        public string NameArea { get; set; }

        /// <summary>
        /// Atr
        /// </summary>
        public List<decimal> Values
        { get; set; }

        /// <summary>
        /// уникальное имя индикатора
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// цвет центрально серии данных (ATR)
        /// </summary>
        public Color ColorBase { get; set; }

        /// <summary>
        /// длинна периода для рассчёта индикатора
        /// </summary>
        public int Lenght;

        /// <summary>
        /// включена ли прорисовка индикатора
        /// </summary>
        public bool PaintOn { get; set; }

        /// <summary>
        /// сохранить настройки в файл
        /// </summary>
        public void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name))
                {
                    return;
                }

                using (StreamWriter writer = new StreamWriter(@"Engine\" + Name + @".txt", false))
                {
                    writer.WriteLine(ColorBase.ToArgb());
                    writer.WriteLine(Lenght);
                    writer.WriteLine(PaintOn);
                    writer.WriteLine(TypeCalculationAverage);
                    writer.Close();
                }
            }
            catch (Exception)
            {
                // отправить в лог
            }
        }

        /// <summary>
        /// загрузить настройки из файла
        /// </summary>
        public void Load()
        {
            if (!File.Exists(@"Engine\" + Name + @".txt"))
            {
                return;
            }
            try
            {

                using (StreamReader reader = new StreamReader(@"Engine\" + Name + @".txt"))
                {
                    ColorBase = Color.FromArgb(Convert.ToInt32(reader.ReadLine()));
                    Lenght = Convert.ToInt32(reader.ReadLine());
                    PaintOn = Convert.ToBoolean(reader.ReadLine());
                    Enum.TryParse(reader.ReadLine(), true, out TypeCalculationAverage);
                    reader.ReadLine();

                    reader.Close();
                }


            }
            catch (Exception)
            {
                // отправить в лог
            }
        }

        /// <summary>
        /// удалить файл с настройками
        /// </summary>
        public void Delete()
        {
            if (File.Exists(@"Engine\" + Name + @".txt"))
            {
                File.Delete(@"Engine\" + Name + @".txt");
            }
        }

        /// <summary>
        /// удалить данные
        /// </summary>
        public void Clear()
        {
            if (Values != null)
            {
                Values.Clear();
            }
            _myCandles = null;
        }

        /// <summary>
        /// показать окно с настройками
        /// </summary>
        public void ShowDialog()
        {
            AtrUi ui = new AtrUi(this);
            ui.ShowDialog();

            if (ui.IsChange && _myCandles != null)
            {
                ProcessAll(_myCandles);

                if (NeadToReloadEvent != null)
                {
                    NeadToReloadEvent(this);
                }
            }
        }

        /// <summary>
        /// свечи для рассчёта индикатора
        /// </summary>
        private List<Candle> _myCandles;

        /// <summary>
        /// рассчитать индикатор
        /// </summary>
        /// <param name="candles">свечи</param>
        public void Process(List<Candle> candles)
        {
            _myCandles = candles;

            if (Values != null &&
                Values.Count + 1 == candles.Count)
            {
                ProcessOne(candles);
            }
            else if (Values != null &&
                     Values.Count == candles.Count)
            {
                ProcessLast(candles);
            }
            else
            {
                ProcessAll(candles);
            }
        }

        /// <summary>
        /// индикатор нужно перерисовать
        /// </summary>
        public event Action<IIndicatorCandle> NeadToReloadEvent;

        /// <summary>
        /// прогрузить только последнюю свечку
        /// </summary>
        private void ProcessOne(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            if (Values == null)
            {
                Values = new List<decimal>();
                Values.Add(GetValue(candles, candles.Count - 1));
            }
            else
            {
                Values.Add(GetValue(candles, candles.Count - 1));
            }
        }

        /// <summary>
        /// прогрузить с самого начала
        /// </summary>
        private void ProcessAll(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            Values = new List<decimal>();

            for (int i = 0; i < candles.Count; i++)
            {
                Values.Add(GetValue(candles, i));
            }
        }

        /// <summary>
        /// перегрузить последнее значение
        /// </summary>
        private void ProcessLast(List<Candle> candles)
        {
            if (candles == null)
            {
                return;
            }
            Values[Values.Count - 1] = GetValue(candles, candles.Count - 1);
        }

        /// <summary>
        /// взять значение индикатора по индексу
        /// </summary>
        /// <param name="candles">свечи</param>
        /// <param name="index">индекс</param>
        /// <returns>значение индикатора по индексу</returns>
        private decimal GetValue(List<Candle> candles,int index)
        {
            TrueRangeReload(candles, index);
            _moving = MovingAverageWild(_trueRange, _moving, Lenght, index);

            return Math.Round(_moving[_moving.Count-1],6);
        }

        private List<decimal> _moving; 

        /// <summary>
        /// истинный диапазон
        /// </summary>
        private List<decimal> _trueRange;

        private void TrueRangeReload(List<Candle> candles, int index)
        {
            //Истинный диапазон (True Range) есть наибольшая из следующих трех величин:
            //разность между текущими максимумом и минимумом;
            //разность между предыдущей ценой закрытия и текущим максимумом;
            //разность между предыдущей ценой закрытия и текущим минимумом.

            if (index == 0)
            {
                _trueRange = new List<decimal>();
                _trueRange.Add(0);
                return;
            }

            if (index > _trueRange.Count - 1)
            {
                _trueRange.Add(0);
            }

            decimal hiToLow = Math.Abs(candles[index].High - candles[index].Low);
            decimal closeToHigh = Math.Abs(candles[index - 1].Close - candles[index].High);
            decimal closeToLow = Math.Abs(candles[index - 1].Close - candles[index].Low);

            _trueRange[_trueRange.Count - 1] = Math.Max(Math.Max(hiToLow, closeToHigh), closeToLow);
        }

        private List<decimal> MovingAverageHard(List<decimal> valuesSeries, List<decimal> moving, int length, int index)
        {
            if (moving == null || length > valuesSeries.Count)
            {
                moving = new List<decimal>();
                for (int i = 0; i < index + 1; i++)
                {
                    moving.Add(0);
                }
            }
            else if (length == valuesSeries.Count)
            { // это первое значение. Рассчитываем как простую машку

                decimal lastMoving = 0;

                for (int i = index; i > valuesSeries.Count - 1 - length; i--)
                {
                    lastMoving += valuesSeries[i];
                }
                if (lastMoving != 0)
                {
                    moving.Add(lastMoving / length);
                }
                else
                {
                    moving.Add(0);
                }

            }
            else
            {
                //decimal a = Math.Round(2.0m / (length * 2), 5);
                decimal a = Math.Round(2.0m / (Lenght + 1), 7);

                decimal lastValueMoving;
                decimal lastValueSeries = Math.Round(valuesSeries[valuesSeries.Count - 1], 7);

                if (index > moving.Count - 1)
                {
                    lastValueMoving = moving[moving.Count - 1];
                    moving.Add(0);
                }
                else
                {
                    lastValueMoving = moving[moving.Count - 2];
                }

                moving[moving.Count - 1] = Math.Round(lastValueMoving + a * (lastValueSeries - lastValueMoving), 7);

            }

            return moving;
        }

        private List<decimal> MovingAverageWild(List<decimal> valuesSeries, List<decimal> moving, int length, int index)
        {
            if (moving == null || length > valuesSeries.Count)
            {
                moving = new List<decimal>();
                for (int i = 0; i < index + 1; i++)
                {
                    moving.Add(0);
                }
            }
            else if (length == valuesSeries.Count)
            { // это первое значение. Рассчитываем как простую машку

                decimal lastMoving = 0;

                for (int i = index; i > -1 && i > valuesSeries.Count - 1 - length; i--)
                {
                    lastMoving += valuesSeries[i];
                }
                if (lastMoving != 0)
                {
                    moving.Add(lastMoving / length);
                }
                else
                {
                    moving.Add(0);
                }

            }
            else
            {

                decimal lastValueMoving;
                decimal lastValueSeries = Math.Round(valuesSeries[valuesSeries.Count - 1], 6);

                if (index > moving.Count - 1)
                {
                    lastValueMoving = moving[moving.Count - 1];
                    moving.Add(0);
                }
                else
                {
                    lastValueMoving = moving[moving.Count - 2];
                }

                moving[moving.Count - 1] = Math.Round((lastValueMoving * (Lenght - 1) + lastValueSeries) / Lenght, 6);

            }

            return moving;
        }


    }
}