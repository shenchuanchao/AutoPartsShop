【Blazor + Ant Design + .NET 8技术栈】

AutoPartsShop/    #汽车配件商城系统
├── AutoPartsShop/          # Blazor WebAssembly 前端（商城前台+管理后台）
	├── Layout/      #布局
		├── AdminLayout.razor
		├── MainLayout.razor
		└── NavMenu.razor
	├── Pages/
		├── Admin/     #管理后台页面
			├── Dashboard.razor
			├── Orders/
			└── Products/
		├── Index.razor
		└── ....razor
	├── _Imports.razor
	├── App.razor
	└── Program.cs
├── AutoPartsShop.API/          # Web API 项目
├── AutoPartsShop.Core/         # 核心业务逻辑
├── AutoPartsShop.Domain/       # 领域模型
├── AutoPartsShop.Identity/ # 
└── AutoPartsShop.Infrastructure/       # 基础设施层
	└── Services/
		├── CartService.cs
		├── ICartService.cs
		├── ProductService.cs
		├── IProductService.cs
		├── OrderService.cs
		└── IOrderService.cs