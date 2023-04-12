using Microsoft.AspNetCore.Mvc;
using TrainBookAPI.Models;

namespace TrainBookAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TrainBookController : ControllerBase
    {
        [HttpPost(Name = "PostTrainBook")]

        public TrainBookReturn PostTrainBook(TrainBookModel trainBookModel)
        {
            TrainBookReturn trainBookReturn = new TrainBookReturn();
            List<YerlesimAyrinti> yerlesimAyrintiList = new List<YerlesimAyrinti>();
            if (trainBookModel != null)
            {
                if (IsAvailable(trainBookModel))
                {
                    if (trainBookModel.KisilerFarkliVagonlaraYerlestirilebilir)
                    {
                        int i = 0;
                        while (i < trainBookModel.RezervasyonYapilacakKisiSayisi)
                        {
                            Random random = new Random();
                            int vagonRandom = random.Next(trainBookModel.Tren.Vagonlar.Count);

                            var selectedCar = trainBookModel.Tren.Vagonlar[vagonRandom];
                            var ratio = Convert.ToDouble((selectedCar.DoluKoltukAdet + 1)) / Convert.ToDouble(selectedCar.Kapasite);
                            if (ratio < 0.7)
                            {
                                if (yerlesimAyrintiList.Count != 0)
                                {
                                    int v = 0;
                                    foreach (var vagondolumu in yerlesimAyrintiList)
                                    {
                                        if (selectedCar.Ad == vagondolumu.VagonAdi)
                                        {
                                            vagondolumu.KisiSayisi += 1;
                                            v++;
                                        }
                                    }
                                    if (v == 0)
                                    {
                                        yerlesimAyrintiList.Add(new YerlesimAyrinti
                                        {
                                            VagonAdi = selectedCar.Ad,
                                            KisiSayisi = 1
                                        });
                                    }
                                }
                                else
                                {
                                    yerlesimAyrintiList.Add(new YerlesimAyrinti
                                    {
                                        VagonAdi = selectedCar.Ad,
                                        KisiSayisi = 1
                                    });
                                }
                                i++;
                            }
                        }
                    }
                    else
                    {
                        bool theEndv = true;
                        while (theEndv)
                        {
                            Random random = new Random();
                            int vagonRandom = random.Next(trainBookModel.Tren.Vagonlar.Count);
                            Vagon secilenVagon = trainBookModel.Tren.Vagonlar[vagonRandom];
                            double ratio = Convert.ToDouble((secilenVagon.DoluKoltukAdet + trainBookModel.RezervasyonYapilacakKisiSayisi)) / Convert.ToDouble(secilenVagon.Kapasite);
                            if (ratio < 0.7)
                            {
                                yerlesimAyrintiList.Add(new YerlesimAyrinti
                                {
                                    VagonAdi = secilenVagon.Ad,
                                    KisiSayisi = trainBookModel.RezervasyonYapilacakKisiSayisi
                                });
                                theEndv = false;
                            }
                        }
                    }
                    trainBookReturn.RezervasyonYapilabilir = true;
                }
                else
                    trainBookReturn.RezervasyonYapilabilir = false;
            }

            trainBookReturn.YerlesimAyrinti = yerlesimAyrintiList;
            return trainBookReturn;
        }
        private bool IsAvailable(TrainBookModel trainBookModel)
        {
            if (!trainBookModel.KisilerFarkliVagonlaraYerlestirilebilir)
            {
                foreach (var item in trainBookModel.Tren.Vagonlar)
                {
                    var ratio = Convert.ToDouble((item.DoluKoltukAdet + trainBookModel.RezervasyonYapilacakKisiSayisi)) / Convert.ToDouble(item.Kapasite);
                    if (ratio < 0.7)
                        return true;
                }
                return false;
            }
            else
            {
                var reservationPeople = trainBookModel.RezervasyonYapilacakKisiSayisi;
                foreach (var item in trainBookModel.Tren.Vagonlar)
                {
                    var doluKoltuk = item.DoluKoltukAdet;
                    while (Convert.ToDouble(doluKoltuk) / Convert.ToDouble(item.Kapasite) < 0.7)
                    {
                        doluKoltuk++;
                        reservationPeople--;
                    }
                }
                if (reservationPeople <= 0)
                    return true;
            }
            return false;
        }
    }
}
