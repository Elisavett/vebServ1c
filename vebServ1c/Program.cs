using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vebServ1c.localhost;
using System.Text.Json;
using System.IO;

//сервис для выгрузки данных в 1С:Предприятие из файла json и заполнение файла
namespace vebServ1c
{
    //данные о выгрузке товаров
    class data
    {
        public DateTime date { get; set; }
        public List<Element> elements { get; set; }
    }
    //Товар
    class Element
    {
        public string elName { get; set; }
        public string supplier { get; set; }
        public int number { get; set; }
        public int price { get; set; }
    }
    class Program
    {
        static void Main(string[] args)
        {
            using (service service = new service())
            {
                //ответ пользователя
                string answ;
                //имя файла для обмена данными
                string fileName = "elements.json";
                do
                {
                    Console.WriteLine("1 - добавить элементы, 2 - получить элементы, 3 - отправить данные");
                    answ = Console.ReadLine();
                    switch (answ)
                    {
                        //добавить элементы
                        case "1":
                            //считываем строку из файла
                            string jsonString = File.ReadAllText(fileName);
                            //проебразовываем в список объектов
                            List<data> data = JsonSerializer.Deserialize<List<data>>(jsonString);

                            List<Element> els = new List<Element>();
                            //добавляем сколько угодно данных,
                            //по кальзователь не откажется
                            do
                            {
                                Element el = new Element();

                                Console.WriteLine("Название элемента: ");
                                el.elName = Console.ReadLine();

                                Console.WriteLine("Поставщик: ");
                                el.supplier = Console.ReadLine();

                                Console.WriteLine("Количество: ");
                                el.number = Convert.ToInt32(Console.ReadLine());

                                Console.WriteLine("Цена: ");
                                el.price = Convert.ToInt32(Console.ReadLine());

                                els.Add(el);

                                Console.WriteLine("Добавить ещё? (y/n)");

                            } while (Console.ReadLine() != "n");

                            //добавить дату довавления и элементы в список
                            data.Add(new data { date = DateTime.Now, elements = els });

                            //преобразовать данные в json-вид
                            jsonString = JsonSerializer.Serialize(data);
                            //записать в файл
                            File.WriteAllText(fileName, jsonString);
                            break;

                        //получить элементы
                        case "2":
                            //считываем строку из файла
                            string json = File.ReadAllText("elements.json");

                            //преобразуем в лист объектов
                            List<data> datas = JsonSerializer.Deserialize<List<data>>(json);

                            //выводим полученные товары
                            foreach (var d in datas)
                            {
                                Console.WriteLine($"Дата: {d.date}");
                                foreach (var el in d.elements)
                                {
                                    Console.WriteLine($"\tНазвание: {el.elName}  Поставщик: {el.supplier} Количество: {el.number} Цена: {el.price}");
                                }
                            }
                            break;

                        //отправить данные
                        case "3":
                            //Задание версии протокола
                            service.SoapVersion = System.Web.Services.Protocols.SoapProtocolVersion.Soap12;

                            //Логин и пароль для авторизации
                            service.Credentials = new System.Net.NetworkCredential("", "");

                            //Считываем все что есть в файле
                            string j = File.ReadAllText("elements.json");

                            //преобразуем считанную строку в лист объектов
                            List<data> datas1 = JsonSerializer.Deserialize<List<data>>(j);

                            //Будем загружать только те данные, которые были добавлены 
                            //в список последней датой. Остальные считаем, что уже загрузили ранее
                            List<Element> elementsList = datas1.Single(da=>da.date == datas1.Max(d => d.date)).elements;
                            
                            //Массив элементов 1С
                            Элементы[] ЭлементыМассив = new Элементы[elementsList.Count];

                            int i = 0;

                            //Переписываем все продукты из списка в массив
                            foreach(var el in elementsList)
                            {
                                ЭлементыМассив[i] = new Элементы();
                                ЭлементыМассив[i].Элемент = el.elName;
                                ЭлементыМассив[i].Поставщик = el.supplier;
                                ЭлементыМассив[i].Количество = el.number.ToString();
                                ЭлементыМассив[i].Цена = el.price.ToString();
                                i++;
                            }
                            //отправляем данные
                            service.setData(ЭлементыМассив);

                            Console.WriteLine("Данные успешно отправлены");
                            break;
                    }
                    Console.WriteLine("Продолжаем? (y/n)");
                } while (Console.ReadLine() != "n");



            }
        }
    }
}
