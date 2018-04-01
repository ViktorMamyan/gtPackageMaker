using Great_Tours_Package_Maker.Excursion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Great_Tours_Package_Maker
{
    public partial class XMLLoaderForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public XMLLoaderForm()
        {
            InitializeComponent();
        }

        string xmlFilePath = @"C:\Users\Vitya\Desktop\transfer ekskursia.xml";

        XDocument xml = null;

        List<Currencies> currencies = new List<Currencies>();
        List<Countries> countries = new List<Countries>();
        List<Services> services = new List<Services>();
        List<RateGroup> rateGroup = new List<RateGroup>();

        private void XMLLoaderForm_Load(object sender, EventArgs e)
        {
            try
            {
                //read xml file
                GetxmlData();

                //get list of currencies
                GetCurrenciesList();

                //get default currency
                GetDefaultCurrency();

                //get list of countries
                GetCountryList();
                //------------------------------------
                SetCountryDataSource();
                //------------------------------------

                //get list of services
                GetServicesList();

                //get list of rates
                GetRateGroupList();

                //join all data (not beeing used)
                //JoinData();

                btnSearch.Enabled = true;
            }
            catch (Exception ex)
            {
                btnSearch.Enabled = false;
                MessageBox.Show(ex.Message, "There is an Exception",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        
        //search click
        private void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedValue == null)
                {
                    throw new Exception("Value not set");
                }

                SearchData(comboBox1.SelectedValue.ToString(), txtPhrase.Text.Trim());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "There is an Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //get xml data
        private void GetxmlData()
        {
            //check if file exists
            if (!File.Exists(xmlFilePath))
            {
                throw new Exception("Source file not found!");
            }
            
            //read xml data
            string xmlData = File.ReadAllText(xmlFilePath);

            //parse xml data
            xml = XDocument.Parse(xmlData);
        }

        //get currency list
        private void GetCurrenciesList()
        {
            var query = from c in xml.Root.Descendants("currency")
                        select new
                        {
                            key = ((c.Attribute("key") == null) ? string.Empty : c.Attribute("key").Value),
                            name = ((c.Attribute("name") == null) ? string.Empty : c.Attribute("name").Value),
                            nameLat = ((c.Attribute("nameLat") == null) ? string.Empty : c.Attribute("nameLat").Value),
                            code = ((c.Attribute("code") == null) ? string.Empty : c.Attribute("code").Value),
                        };
            
            currencies.Clear();
            foreach (var item in query)
            {
                currencies.Add(new Currencies {Key=item.key,Name=item.name,NameLat=item.nameLat,Code=item.code,IsDefault=false});
            }
            if (currencies.Count == 0)
            {
                throw new Exception("Currency list not defined!");
            }
        }

        //get default currency
        private void GetDefaultCurrency()
        {
            var query = from c in xml.Root.Descendants("defaultCurrency")
                        select new
                        {
                            currency = ((c.Attribute("currency") == null) ? string.Empty : c.Attribute("currency").Value)
                        };

            if (query.Count() > 0)
            {
                string defCurrency=string.Empty;
                bool idDefaultSet = false;

                foreach (var item in query)
                {
                    defCurrency = item.currency;
                    break;
                }
                foreach (var item in currencies)
                {
                    if (item.Key == defCurrency)
                    {
                        item.IsDefault = true;
                        idDefaultSet = true;
                        break;
                    }
                }
                if (idDefaultSet==false)
                {
                    currencies[0].IsDefault = true;
                }
            }
            else
            {
                currencies[0].IsDefault = true;
            }
        }

        //get country list
        private void GetCountryList()
        {
            var query = from c in xml.Root.Descendants("country")
                        select new
                        {
                            key = ((c.Attribute("key") == null) ? string.Empty : c.Attribute("key").Value),
                            name = ((c.Attribute("name") == null) ? string.Empty : c.Attribute("name").Value),
                            nameLat = ((c.Attribute("nameLat") == null) ? string.Empty : c.Attribute("nameLat").Value),
                            code = ((c.Attribute("code") == null) ? string.Empty : c.Attribute("code").Value),
                        };

            countries.Clear();
            foreach (var item in query)
            {
                countries.Add(new Countries { Key = item.key, Name = item.name, NameLat = item.nameLat, Code = item.code});
            }
            if (countries.Count == 0)
            {
                throw new Exception("Country list not defined!");
            }
        }

        //get services list
        private void GetServicesList()
        {
            var query = from c in xml.Root.Descendants("service")
                        select new
                        {
                            countryKey = ((c.Attribute("countryKey") == null) ? string.Empty : c.Attribute("countryKey").Value),
                            name = ((c.Attribute("name") == null) ? string.Empty : c.Attribute("name").Value),
                            key = ((c.Attribute("key") == null) ? string.Empty : c.Attribute("key").Value)
                        };

            services.Clear();
            foreach (var item in query)
            {
                string city = string.Empty;
                int index = item.name.LastIndexOf("-");

                if (index > 0)
                {
                    city = (item.name.Substring(index + 1, item.name.Length - (index + 1))).Trim();
                }
                else
                {
                    city = "N/A";
                }

                services.Add(new Services { CountryKey=item.countryKey,Name=item.name,Key=item.key,City=city });
            }
            if (services.Count == 0)
            {
                throw new Exception("Excursion list not defined!");
            }  
        }

        //get rateGroup and rate list
        private void GetRateGroupList()
        {
            var query = from c in xml.Root.Descendants("rateGroup")
                        select new
                        {
                            serviceKey = ((c.Attribute("serviceKey") == null) ? string.Empty : c.Attribute("serviceKey").Value)
                        };

            rateGroup.Clear();
            foreach (var item in query)
            {
                List<Rate> rts = new List<Rate>();

                if (item.serviceKey != string.Empty)
                {
                    var query2 = from c in xml.Root.Descendants("rateGroup")
                                where c.Attribute("serviceKey").Value.Equals(item.serviceKey.ToString())
                                select c;

                    XDocument xml3 = XDocument.Parse(query2.ForEachToString());
                    var query3 = from c in xml3.Root.Descendants("rate")
                                 select new
                                 {
                                     per = ((c.Attribute("per") == null) ? string.Empty : c.Attribute("per").Value),
                                     fill = ((c.Attribute("fill") == null) ? string.Empty : c.Attribute("fill").Value),
                                     @group = ((c.Attribute("group") == null) ? string.Empty : c.Attribute("group").Value),
                                     value = ((c.Attribute("value") == null) ? string.Empty : c.Attribute("value").Value)
                                 };
                    foreach (var item2 in query3)
                    {
                        Rate r = new Rate { Per=item2.per,Fill=item2.fill,Group=item2.group,Value=item2.value };
                        rts.Add(r);
                    }
                    rateGroup.Add(new RateGroup { ServiceKey = item.serviceKey, Rates = rts });
                }
            }
            if (rateGroup.Count == 0)
            {
                throw new Exception("Rate list not defined!");
            }
        }

        //join data (not beeing used)
        private void JoinData()
        {
            var query = from c in countries
                        join s in services
                            on c.Key equals s.CountryKey into rt
                            from s in rt.DefaultIfEmpty()
                        orderby c.Name
                            select new
                            {
                                CountryKey = c.Key,
                                CountryName = c.Name,
                                CountryLatName = c.NameLat,
                                CountryCode = c.Code,
                                ServiceName = s == null ? string.Empty : s.Name,
                                ServiceCity = s == null ? string.Empty : s.City,
                                ServiceKey = s == null ? string.Empty : s.Key
                            };

            gridControl1.DataSource = query;
        }

        //join service and rate
        private void SearchData(string CountryKey,string Phrase)
        {
            List<ReadyExcursion> readyExcursion = new List<ReadyExcursion>();

            foreach (var item in services.Where(x => x.CountryKey == CountryKey && x.Name.ToLower().Contains(Phrase.ToLower())))
            {
                var query = rateGroup.Where(x => x.ServiceKey == item.Key);
                List<Rate> r = new List<Rate>();

                foreach (var item2 in query)
                {
                    foreach (var item3 in item2.Rates)
                    {
                        r.Add(new Rate {Per =item3.Per, Fill = item3.Fill , Group = item3.Group, Value = item3.Value});
                    }
                }

                readyExcursion.Add(new ReadyExcursion { ServiceName = item.Name, Rates = r });
            }

            gridControl1.BeginUpdate();
            gridControl1.DataSource = null;
            gridView1.Columns.Clear();
            
            gridControl1.DataSource = readyExcursion.OrderBy(x => x.ServiceName);

            for (int i = 0; i < gridView1.Columns.Count(); i++)
			{
                gridView1.Columns[i].OptionsFilter.FilterPopupMode = DevExpress.XtraGrid.Columns.FilterPopupMode.CheckedList;
			}

            if (gridView1.Columns["ServiceName"].Summary.ActiveCount == 0)
            {
                DevExpress.XtraGrid.GridColumnSummaryItem itemStatus = new DevExpress.XtraGrid.GridColumnSummaryItem(DevExpress.Data.SummaryItemType.Count, "ProductName", "Count: {0}");
                gridView1.Columns["ServiceName"].Summary.Add(itemStatus);
            }

            gridView1.ClearSelection();
            gridControl1.EndUpdate();

        }

        //set country datasource
        private void SetCountryDataSource()
        {
            comboBox1.DataSource = countries;
            comboBox1.DisplayMember = "name";
            comboBox1.ValueMember = "key";
        }

    }
}