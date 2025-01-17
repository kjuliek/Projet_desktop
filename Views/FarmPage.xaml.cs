using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Diagnostics;

using Projet_desktop.Models;
using System.Linq;
using System.Security.Permissions;

namespace Projet_desktop.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    internal class FarmPlot
    {
        public int              Row      { get; set; }
        public int              Column   { get; set; }
        public StorageBuilding? Building { get; set; }

    }

    public partial class FarmPage : Page
    {
        private User                    user;
        private List<BuildingType>      building_types       = [];
        private List<ProductType>       product_types        = [];
        private List<PlantSpecies>      plant_species        = [];
        private List<AnimalSpecies>     animal_species       = [];
        private List<StorageBuilding>   storage_buildings    = [];
        private List<LiveStockBuilding> live_stock_buildings = [];
        private List<Field>             fields               = [];
        private List<Product>           products             = [];
        private List<Crop>              crops                = [];
        private List<Animal>            animals              = [];


        private bool   placementModeActivated = false;
        private string typeValue;
        private string nameValue;
        private int    idValue;
        private int    priceValue;
        private int    rows = 20;
        private int    columns = 40;


        public FarmPage()
        {
            InitializeComponent();
            LoadData();
            LoadShop();
            LoadDashBoard();
            UpdateDateText();
            UpdateMoneyText();
            LoadPlacementText();
            LoadFarmGrid(rows, columns);
            LoadBuildings();
        }

        private void LoadData()
        {
            // User
            user   = new();

            // Building
            building_types       = BuildingType.LoadBuildingTypeFromDB();
            storage_buildings    = StorageBuilding.LoadBuildingFromDB();
            live_stock_buildings = LiveStockBuilding.LoadLiveStockBuildingFromDB();

            // Product
            product_types        = ProductType.LoadProductTypeFromDB();
            products             = Product.LoadProductFromDB();

            // Plant
            plant_species        = PlantSpecies.LoadPlantSpeciesFromDB();
            crops                = Crop.LoadAnimalFromDB();

            // Animal
            animal_species       = AnimalSpecies.LoadAnimalSpeciesFromDB();
            animals              = Animal.LoadAnimalFromDB();

        }

        private void LoadShop()
        {
            // Building
            var building_summaries = building_types.Select(b => new
            {
                id       = b.BuildingTypeID,
                type     = "building",
                name     = b.Type,
                rate     = "",
                p_time   = "",
                average  = "",
                b_time   = b.BuildTime,
                capacity = b.Capacity,
                product  = "",
                building = "",
                price    = b.PurchasePrice
            }).ToList();
            BuildingShopGrid.ItemsSource = building_summaries;

            // Product
            var product_summaries = product_types.Select(p => new
            {
                id       = p.ProductTypeID,
                type     = "product",
                name     = p.Name,
                rate     = p.ProductionRate,
                p_time   = p.ProductionTime,
                average  = "",
                b_time   = "",
                capacity = "",
                product  = "",
                building = p.BuildingType.Type,
                price    = p.Price
            }).ToList();
            ProductShopGrid.ItemsSource = product_summaries;

            // Plants
            var seed_summaries = plant_species.Select(p => new
            {
                id       = p.PlantSpeciesID,
                type     = "seed",
                name     = p.Name,
                rate     = "",
                p_time   = "",
                average  = "",
                b_time   = "",
                capacity = "",
                product  = p.ProductType.Name,
                building = p.BuildingType.Type,
                price    = p.PurchasePrice
            }).ToList();
            SeedShopGrid.ItemsSource = seed_summaries;

            // Animals
            var animal_summaries = animal_species.Select(a => new
            {
                id       = a.AnimalSpeciesID,
                type     = "animal",
                name     = a.Name,
                rate     = "",
                p_time   = "",
                average  = a.AverageAge,
                b_time   = "",
                capacity = "",
                product  = a.ProductType.Name,
                building = a.BuildingType.Type,
                price    = a.PurchasePrice
            }).ToList();
            AnimalShopGrid.ItemsSource = animal_summaries;

            // All
            var all_summaries = new List<object>()
                .Concat(building_summaries)
                .Concat(product_summaries)
                .Concat(seed_summaries)
                .Concat(animal_summaries)
                .ToList();
            AllShopGrid.ItemsSource = all_summaries;
        }

        private void LoadDashBoard()
        {
            // Building
            var storage_building_summaries = storage_buildings.Select(sb => new
            {
                type     = sb.BuildingType.Type,
                build    = sb.TimeUntilBuild,
                products = sb.Products.Count(),
                capacity = sb.BuildingType.Capacity
            }).ToList();

            var live_stock_building_summaries = live_stock_buildings.Select(lsb => new
            {
                type     = lsb.BuildingType.Type,
                build    = lsb.TimeUntilBuild,
                products = lsb.Products.Count(),
                animals  = lsb.Animals.Count(),
                capacity = lsb.BuildingType.Capacity
            }).ToList();

            var building_summaries = new List<object>()
                .Concat(storage_building_summaries)
                .Concat(live_stock_building_summaries)
                .ToList();
            BuildingDashBoardGrid.ItemsSource = building_summaries;

            // Product
            var product_summaries = products.GroupBy(p => p.ProductType.Name)
               .Select(group => new
               {
                   type     = group.Key,
                   products = group.Count()
               }).ToList();
            ProductDashBoardGrid.ItemsSource = product_summaries;

            // Crop
            var crop_summaries = crops.Select(c =>
            {
                var field = fields.FirstOrDefault(f => f.Crops.Any(fc => fc.CropID == c.CropID));

                return new
                {
                    type     = c.PlantSpecies.Name,
                    age      = c.GrowthTime,
                    harvest  = c.PlantSpecies.ProductType.ProductionTime - c.GrowthTime,
                    water    = field?.Water?? 0,
                    thirsty  = c.Thirsty,
                    crops    = field?.Crops.Count?? 0,
                    products = field?.Products.Count?? 0,
                    capacity = field?.BuildingType.Capacity?? 0
                };
            }).ToList();
            CropDashBoardGrid.ItemsSource = crop_summaries;

            // Animals
            var animal_summaries = animals.Select(a =>
            {
                var ls_building = live_stock_buildings.FirstOrDefault(lsb => lsb.Animals.Any(a => a.AnimalID == a.AnimalID));

                return new
                {
                    type        = a.AnimalSpecies.Name,
                    age         = a.Age,
                    harvest     = a.TimeUntilHarvest,
                    food        = ls_building?.Food?? 0,
                    hungry      = a.Hungry,
                    water       = ls_building?.Water?? 0,
                    thirsty     = a.Thirsty,
                    cleanliness = ls_building?.Cleanliness?? 0,
                    clean       = a.Clean,
                    animals     = ls_building?.Animals.Count?? 0,
                    products    = ls_building?.Products.Count?? 0,
                    capacity    = ls_building?.BuildingType.Capacity?? 0
                };
            }).ToList();
            AnimalDashBoardGrid.ItemsSource = animal_summaries;

            // All
            var all_summaries = new List<object>()
                .Concat(building_summaries)
                .Concat(crop_summaries)
                .Concat(animal_summaries)
                .Concat(product_summaries)
                .ToList();
            AllDashBoardGrid.ItemsSource = all_summaries;
        }

        private void LoadFarmGrid(int rows, int columns)
        {

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    Button button = new Button
                    {
                        Tag    = new FarmPlot { Row = i, Column = j },
                        Margin = new Thickness(1),
                        Height = 40,
                        Width  = 40
                    };
                    button.Click += FarmPlotButton_Click;
                    FarmGrid.Children.Add(button);
                }
            }
            CenterView();
        }

        private void LoadBuildings()
        {
            foreach (StorageBuilding building in storage_buildings)
            {
                int index = building.PositionY * columns + building.PositionX;
                UIElement selectedElement = FarmGrid.Children[index];
                if (selectedElement is Button button)
                {
                    if (button.Tag is FarmPlot b)
                        b.Building = building;
                    button.Content = building.BuildingType.Type;
                }
            }
            foreach (LiveStockBuilding building in live_stock_buildings)
            {
                int index = building.PositionY * columns + building.PositionX;
                UIElement selectedElement = FarmGrid.Children[index];
                if (selectedElement is Button button)
                {
                    if (button.Tag is FarmPlot b)
                        b.Building = building;
                    button.Content = building.BuildingType.Type;
                }
            }
            foreach (Field building in fields)
            {
                int index = building.PositionY * columns + building.PositionX;
                UIElement selectedElement = FarmGrid.Children[index];
                if (selectedElement is Button button)
                {
                    if (button.Tag is FarmPlot b)
                        b.Building = building;
                    button.Content = building.BuildingType.Type;
                }
            }
        }

        private void UpdateDateText()
        {
            DateTextBlock.Text = user.Date.ToString("dddd dd MMMM yyyy");
        }

        private void UpdateMoneyText()
        {
            MoneyTextBlock.Text = user.Money.ToString() + " pieces";
        }

        private void LoadPlacementText()
        {
            PlacementTextBlock.Text = "Select a position on the map to place the building.";
        }

        private void CenterView()
        {
            GameScrollViewer.Loaded += (s, e) =>
            {
                double verticalOffset   = (FarmGrid.ActualHeight - GameScrollViewer.ActualHeight) / 2;
                double horizontalOffset = (FarmGrid.ActualWidth  - GameScrollViewer.ActualWidth)  / 2;

                GameScrollViewer.ScrollToVerticalOffset(Math.Max(verticalOffset, 0));
                GameScrollViewer.ScrollToHorizontalOffset(Math.Max(horizontalOffset, 0));
            };
        }

        private void FarmPlotButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            if (placementModeActivated)
            {
                if (clickedButton.Tag is FarmPlot plot && plot.Building == null)
                {
                    int x = plot.Column;
                    int y = plot.Row;

                    int buildings_count = fields.Count() + storage_buildings.Count() + live_stock_buildings.Count();
                    if (nameValue == "Field")
                        fields.Add(new Field(idValue, buildings_count, x, y));

                    else if (nameValue == "Storage")
                        storage_buildings.Add(new StorageBuilding(idValue, buildings_count, x, y));

                    else
                        live_stock_buildings.Add(new LiveStockBuilding(idValue, buildings_count, x, y));

                    placementModeActivated = false;

                    ShopContainer.Visibility      = Visibility.Visible;
                    Shop.Visibility               = Visibility.Visible;
                    DashBoard.Visibility          = Visibility.Visible;
                    Param.Visibility              = Visibility.Visible;
                    DateTextBlock.Visibility      = Visibility.Visible;
                    NextDay.Visibility            = Visibility.Visible;
                    MoneyTextBlock.Visibility     = Visibility.Visible;
                    PlacementTextBlock.Visibility = Visibility.Collapsed;
                    LoadDashBoard();
                    UpdateMoneyText();
                    LoadBuildings();
                }
                else
                {
                    MessageBox.Show("This place already has a building.");
                }
            }
            else
            {
                if (clickedButton.Tag is FarmPlot plot && plot.Building != null)
                {
                    if (plot.Building.BuildingType.Type == "Field")
                    {
                        if (plot.Building is Field building)
                        {
                            FieldContainer.Visibility  = Visibility.Visible;
                            FieldProductTextBlock.Text = building.Products.Count.ToString() + " products";
                            FieldCropTextBlock.Text    = building.Crops.Count.ToString() + " crops";
                            FieldWaterTextBlock.Text   = "Water : " + building.Water;
                            FieldStore_Button.Tag      = building;
                            FieldWater_Button.Tag      = building;
                        }
                    }
                    else if ((plot.Building.BuildingType.Type == "ChickenCoop") ||
                             (plot.Building.BuildingType.Type == "SheepBarn") ||
                             (plot.Building.BuildingType.Type == "CowBarn"))
                    {
                        if (plot.Building is LiveStockBuilding building)
                        {
                            LiveStockContainer.Visibility      = Visibility.Visible;
                            LiveStockProductTextBlock.Text     = building.Products.Count.ToString() + " products";
                            LiveStockAnimalTextBlock.Text      = building.Animals.Count.ToString() + " animals";
                            LiveStockFoodTextBlock.Text        = "Food : " + building.Food;
                            LiveStockWaterTextBlock.Text       = "Water : " + building.Water;
                            LiveStockCleanlinessTextBlock.Text = "Cleanliness : " + building.Cleanliness;
                            LiveStockStore_Button.Tag          = building;
                            LiveStockFood_Button.Tag           = building;
                            LiveStockWater_Button.Tag          = building;
                            LiveStockClean_Button.Tag          = building;
                        }
                    }
                    else if (plot.Building.BuildingType.Type == "Storage")
                    {
                        StorageContainer.Visibility  = Visibility.Visible;
                        StorageProductTextBlock.Text = plot.Building.Products.Count.ToString() + " products";
                        SellButton.Tag               = plot.Building;

                    }
                }
            }
        }

        // Click Functions

        private void Move_Click(object sender, RoutedEventArgs e)
        {
            const double scrollAmount = 100;
            if (sender is Button button)
            {
                switch (button.Content.ToString())
                {
                    case "↑":
                        GameScrollViewer.ScrollToVerticalOffset(GameScrollViewer.VerticalOffset - scrollAmount);
                        break;
                    case "↓":
                        GameScrollViewer.ScrollToVerticalOffset(GameScrollViewer.VerticalOffset + scrollAmount);
                        break;
                    case "←":
                        GameScrollViewer.ScrollToHorizontalOffset(GameScrollViewer.HorizontalOffset - scrollAmount);
                        break;
                    case "→":
                        GameScrollViewer.ScrollToHorizontalOffset(GameScrollViewer.HorizontalOffset + scrollAmount);
                        break;
                }
            }
        }
        private void CloseShop_Click(object sender, RoutedEventArgs e)
        {
            ShopContainer.Visibility = Visibility.Collapsed;
        }


        private void OpenShop_Click(object sender, RoutedEventArgs e)
        {
            ShowShop();
        }

        private void Next_Day_Click(object sender, RoutedEventArgs e)
        {
            user.Date = user.Date.AddDays(1);
            foreach (Field field in fields)
            {
                foreach (Crop crop in field.Crops)
                {
                    crop.GrowthTime += 1;
                    if (crop.GrowthTime == crop.PlantSpecies.ProductType.ProductionTime && field.Products.Count != field.BuildingType.Capacity)
                    {
                        Product p = new(crop.PlantSpecies.ProductType.ProductTypeID, products.Count);
                        field.Products.Add(p);
                        products.Add(p);
                        field.Crops.Remove(crop);
                        crops.Remove(crop);
                    }
                    else if (field.Products.Count == field.BuildingType.Capacity)
                        crop.GrowthTime -= 1;
                    if (field.Water > 0)
                    {
                        field.Water -= 1;
                        if (crop.Thirsty)
                            crop.Thirsty = false;
                    } 
                    else
                    {
                        if (crop.Thirsty)
                        {
                            field.Crops.Remove(crop);
                            crops.Remove(crop);
                        }
                        else
                            crop.Thirsty = true;
                    }
                }
            }
            foreach (LiveStockBuilding building in live_stock_buildings)
            {
                foreach (Animal animal in building.Animals)
                {
                    animal.Age += 1;
                    animal.TimeUntilHarvest -= 1;
                    if (animal.TimeUntilHarvest == 0 && building.Products.Count != building.BuildingType.Capacity)
                    {
                        Product p = new(animal.AnimalSpecies.ProductType.ProductTypeID, products.Count);
                        building.Products.Add(p);
                        products.Add(p);
                        animal.TimeUntilHarvest = animal.AnimalSpecies.ProductType.ProductionTime;
                    }
                    else if (building.Products.Count != building.BuildingType.Capacity)
                        animal.TimeUntilHarvest += 1;
                    if (animal.Age == animal.AnimalSpecies.AverageAge)
                    {
                        building.Animals.Remove(animal);
                        animals.Remove(animal);
                    }
                    if (building.Food > 0)
                    {
                        building.Food -= 1;
                        if (animal.Hungry)
                            animal.Hungry = false;
                    }
                    else
                    {
                        if (animal.Hungry)
                        {
                            building.Animals.Remove(animal);
                            animals.Remove(animal);
                        }
                        else
                            animal.Hungry = true;
                    }
                    if (building.Water > 0)
                    {
                        building.Water -= 1;
                        if (animal.Thirsty)
                            animal.Thirsty = false;
                    }
                    else
                    {
                        if (animal.Thirsty)
                        {
                            building.Animals.Remove(animal);
                            animals.Remove(animal);
                        }
                        else
                            animal.Thirsty = true;
                    }
                    if (building.Cleanliness > 0)
                    {
                        building.Cleanliness -= 1;
                        if (animal.Clean)
                            animal.Clean = false;
                    }
                    else
                    {
                        animal.Clean = true;
                    }
                }
            }
            UpdateDateText();
        }

        private void CloseDashBoard_Click(object sender, RoutedEventArgs e)
        {
            DashBoardContainer.Visibility = Visibility.Collapsed;
        }

        private void OpenDashBoard_Click(object sender, RoutedEventArgs e)
        {
            ShowDashBoard();
        }
        private void CloseParam_Click(object sender, RoutedEventArgs e)
        {
            ParamContainer.Visibility = Visibility.Collapsed;
        }
        private void CloseLiveStock_Click(object sender, RoutedEventArgs e)
        {
            LiveStockContainer.Visibility = Visibility.Collapsed;
        }
        private void CloseStorage_Click(object sender, RoutedEventArgs e)
        {
            StorageContainer.Visibility = Visibility.Collapsed;
        }
        private void CloseField_Click(object sender, RoutedEventArgs e)
        {
            FieldContainer.Visibility = Visibility.Collapsed;
        }
        private void OpenParam_Click(object sender, RoutedEventArgs e)
        {
            ParamShop();
        }
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.MainFrame.Navigate(new MainMenu());
        }

        private void Sell_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is StorageBuilding building)
            {
                for (int i = building.Products.Count - 1; i >= 0; i--)
                {
                    var product = building.Products[i];
                    building.Products.RemoveAt(i);
                    user.Money += product.ProductType.Price;
                    products.Remove(product);
                    StorageProductTextBlock.Text = building.Products.Count.ToString() + " products";
                    LoadDashBoard();
                }
            }
        }
        private void Store_Products_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is StorageBuilding building)
            {
                for (int i = building.Products.Count - 1; i >= 0; i--)
                {
                    if (storage_buildings.Count == 0)
                    {
                        MessageBox.Show("You should have storage building to store products.");
                        return;
                    }
                    var product = building.Products[i];
                    bool added = false;
                    foreach (StorageBuilding storage in storage_buildings)
                    {
                        if (storage.Products.Count != storage.BuildingType.Capacity && !added)
                        {
                            storage.Products.Add(product);
                            added = true;
                        }
                    }
                    if (!added)
                    {
                        MessageBox.Show("You should have more storage buildings to store products.");
                        return;
                    }
                    
                    building.Products.RemoveAt(i);
                    LiveStockProductTextBlock.Text = building.Products.Count.ToString() + " products";
                }
            }
        }
        private void Feed_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is LiveStockBuilding building)
            {
                building.Food = 5;
                LiveStockFoodTextBlock.Text = "Food : " + building.Food;
            }
        }
        private void Water_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is LiveStockBuilding building)
            {
                building.Water = 5;
                LiveStockWaterTextBlock.Text = "Water : " + building.Water;
            }
            else if (button?.Tag is Field field)
            {
                field.Water = 5;
                FieldWaterTextBlock.Text = "Water : " + field.Water;
            }
        }
        private void Clean_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is LiveStockBuilding building)
            {
                building.Cleanliness = 10;
                LiveStockCleanlinessTextBlock.Text = "Cleanliness : " + building.Cleanliness;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            user.SaveUserToDB();
            foreach (Product product in products)
            {
                product.SaveProductToDB();
            }
            foreach (StorageBuilding building in storage_buildings)
            {
                building.SaveBuildingToDB();
            }
            foreach (LiveStockBuilding building in live_stock_buildings)
            {
                building.SaveBuildingToDB();
            }
            foreach (Field building in fields)
            {
                building.SaveBuildingToDB();
            }
            foreach (Crop crop in crops)
            {
                crop.SaveCropToDB();
            }
            foreach (Animal animal in animals)
            {
                animal.SaveAnimalToDB();
            }
        }

        private void Shop_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            if (button?.Tag is not null)
            {
                var rowData = button.Tag;

                typeValue = rowData.GetType().GetProperty("type")?.GetValue(rowData)?.ToString();
                nameValue = rowData.GetType().GetProperty("name")?.GetValue(rowData)?.ToString();
                idValue = int.Parse(rowData.GetType().GetProperty("id")?.GetValue(rowData)?.ToString() ?? "");
                priceValue = int.Parse(rowData.GetType().GetProperty("price")?.GetValue(rowData)?.ToString() ?? "");

                if (user.Money < priceValue)
                    MessageBox.Show($"No such money");
                else
                {
                    if (typeValue == "product")
                    {
                        if (storage_buildings.Count == 0)
                        {
                            MessageBox.Show("You should have storage building to buy products.");
                            return;
                        }
                        Product product = new(idValue, products.Count);
                        bool    added   = false;
                        foreach (StorageBuilding storage in storage_buildings)
                        {
                            if (storage.Products.Count != storage.BuildingType.Capacity && !added)
                            {
                                storage.Products.Add(product);
                                added = true;
                            }
                        }
                        if (!added)
                        {
                            MessageBox.Show("You should have more fields to buy seeds.");
                            return;
                        }
                        products.Add(product);
                    }
                    else if (typeValue == "building")
                    {
                        EnterPlacementMode();
                    }
                    else if (typeValue == "seed")
                    {
                        if (fields.Count() == 0)
                        {
                            MessageBox.Show("You should have fields to buy seeds.");
                            return;
                        }
                        Crop crop  = new(idValue, crops.Count);
                        bool added = false;
                        foreach (Field field in fields)
                        {
                            if (field.Crops.Count != field.BuildingType.Capacity && !added)
                            {
                                field.Crops.Add(crop);
                                added = true;
                            }
                        }
                        if (!added)
                        {
                            MessageBox.Show("You should have more fields to buy seeds.");
                            return;
                        }
                        crops.Add(crop);
                    }
                    else if (typeValue == "animal")
                    {
                        Animal animal            = new(idValue, animals.Count);
                        bool   added             = false;
                        var    matchingBuildings = live_stock_buildings.Where(building => building.BuildingType.Type == animal.AnimalSpecies.BuildingType.Type).ToList();
                        if (matchingBuildings.Count == 0)
                        {
                            MessageBox.Show($"You should have {animal.AnimalSpecies.BuildingType.Type} to buy {animal.AnimalSpecies.Name}.");
                            return;
                        }
                        foreach (LiveStockBuilding building in matchingBuildings)
                        {
                            if (building.Animals.Count != building.BuildingType.Capacity && !added)
                            {
                                building.Animals.Add(animal);
                                added = true;
                            }
                        }
                        if (!added)
                        {
                            MessageBox.Show($"You should have more {animal.AnimalSpecies.BuildingType.Type} to buy {animal.AnimalSpecies.Name}.");
                            return;
                        }
                        animals.Add(animal);
                    }
                    MessageBox.Show($"You buy a {typeValue} : {nameValue} to {priceValue} $");
                }
            }
            else
            {
                MessageBox.Show("No data found for the clicked button.");
            }

            user.Money -= priceValue;
            LoadDashBoard();
            UpdateMoneyText();
        }

        // Shop PopUp
        private void ShowShop()
        {
            ShopContainer.Visibility = Visibility.Visible;
        }

        private void ShopContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == ShopContainer)
            {
                CloseShop_Click(sender, e);
            }
        }
        
        // DashBoard PopUp
        private void ShowDashBoard()
        {
            DashBoardContainer.Visibility = Visibility.Visible;
        }

        private void DashBoardContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == DashBoardContainer)
            {
                CloseDashBoard_Click(sender, e);
            }
        }

        // Param PopUp
        private void ParamShop()
        {
            ParamContainer.Visibility = Visibility.Visible;
        }

        private void ParamContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == ParamContainer)
            {
                CloseParam_Click(sender, e);
            }
        }

        private void EnterPlacementMode()
        {
            placementModeActivated = true;

            ShopContainer.Visibility      = Visibility.Collapsed;
            Shop.Visibility               = Visibility.Collapsed;
            DashBoard.Visibility          = Visibility.Collapsed;
            Param.Visibility              = Visibility.Collapsed;
            DateTextBlock.Visibility      = Visibility.Collapsed;
            NextDay.Visibility            = Visibility.Collapsed;
            MoneyTextBlock.Visibility     = Visibility.Collapsed;
            PlacementTextBlock.Visibility = Visibility.Visible;
        }
    }
}