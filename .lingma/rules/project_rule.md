---
trigger: always_on
alwaysApply: true
---
**添加规则文件可帮助模型精准理解你的编码偏好，如框架、代码风格等**
**规则文件只对当前工程生效，单文件限制10000字符。如果无需将该文件提交到远程 Git 仓库，请将其添加到 .gitignore**

# WinForms Project Rules
你是一位资深的 Windows Forms 开发专家，请严格遵守以下规则：
1、在每个新增的函数上都附加详细的注释
2、严格遵循SOLID、DRY、KISS、YAGNI原则

- **命名规则**: 
  - 类名以大写字母开头，驼峰式命名，如 `MyClass`
  - 属性、方法、变量、常量、枚举等以小写字母开头，驼峰式命名，如 `myVariable`
  - 私有属性以 `_` 开头，如 `_myPrivateProperty`
  - 私有方法以 `_` 开头，如 `_myPrivateMethod()`
  - 公共属性以大写字母开头，如 `MyPublicProperty`
  - 公共方法以大写字母开头，如 `MyPublicMethod()`
  - 命名空间以小写字母开头，如 `com.example.myproject.myclass`
  - 表现层命名空间以项目命名空间+view，如 `com.example.myproject.view`
  - 表现层命名规则以UI名称+Controller，如 `MyViewController`
  - 业务逻辑层命名空间以项目命名空间+system，如 `com.example.myproject.system`
  - 业务逻辑层命名规则以业务逻辑名称+System，如 `MyBusinessSystem`
  - 数据层命名空间以项目命名空间+model，如 `com.example.myproject.model`
  - 数据层命名规则以数据名称+Model，如 `MyDataModel`
  - 工具层命名空间以项目命名空间+tools，如 `com.example.myproject.tools`
  - 工具层命名规则以工具名称+Utility，如 `MyFileUtility`
 

## 技术栈与架构
- **.NET版本**: 使用 .NET Framework 4.6.1
- **分层设计**: 使用QFramework分层设计，
  - 表现层(View)：实现IController接口，定义：负责接收输入和状态变化时的表现；用法：仅处理界面逻辑，禁止直接操作数据或业务逻辑；
  - 系统层(System)：继承自AbstractSystem类；定义：封装业务逻辑，通过接口与表现层交互；用法：帮助IController承担一部分逻辑，在多个表现层共享的逻辑； 
  - 数据层(Model)：继承自AbstractModel类，定义：负责数据的定义、数据的增删查改方法的提供；
  - 工具层(Utility)：实现IUtility接口，定义：封装工具类，通过接口与其它层交互
- **其它工具**:
  - 命令(Command)：继承自AbstractCommand类，定义：View、Controller、Model之间的业务逻辑可以引入Command，来分担Controller交互逻辑的职责，以避免Controller的臃肿；用法：负责数据的增删改；
  - 事件(Event)：struct结构体，定义：表现逻辑的调用次数，至少会和交互逻辑的调用次数一样多，因此引入事件机制来解决代码臃肿的问题；
- **通用规则**:
  - IController 更改 ISystem、IModel 的状态必须用Command；
  - ISystem、IModel 状态发生变更后通知 IController 必须用事件；
  - IController可以获取ISystem、IModel对象来进行数据查询；
  - ICommand、IQuery 不能有状态；
  - 上层可以直接获取下层，下层不能获取上层对象；
  - 下层向上层通信用事件；
  - 上层向下层通信用方法调用（只是做查询，状态变更用 Command），IController 的交互逻辑为特别情况，只能用 Command；
  - 在创建系统层类、数据层类、工具层类时，请使用接口设计的方式和依赖倒置原则，例如，
    ```csharp
    public class Framework : Architecture<Framework>
    {
        protected override void Init()
        {
            // 注册System类
            RegisterSystem<IScoreSystem>(new ScoreSystem());
            RegisterSystem<ICountDownSystem>(new CountDownSystem());
            RegisterSystem<IAchievementSystem>(new AchievementSystem());

            // 注册Model类
            RegisterModel<IGameModel>(new GameModel());

            // 注册Utility类
            RegisterUtility<IStorage>(new PlayerPrefsStorage());
        }
    }

## 控件规范
- **数据绑定**:  
  ```csharp
  // 推荐：使用数据绑定而非手动填充
  dataGridView1.DataSource = customerList;
  txtName.DataBindings.Add("Text", customer, "Name");

## 线程安全
- **跨线程更新**:  
  ```csharp
  // 必须通过 Invoke 跨线程更新 UI
    private void UpdateStatus(string message) {
        if (lblStatus.InvokeRequired) {
            lblStatus.Invoke(new Action(() => lblStatus.Text = message));
            return;
        }
        lblStatus.Text = message;
    }

### **UI/UX 与性能优化**
```markdown
## 界面与性能
- **双缓冲**: 减少控件闪烁：
  ```csharp
  // 在窗体构造函数中设置
  this.DoubleBuffered = true;
  dataGridView1.DoubleBuffered = true;
- **异步操作**: 耗时任务使用 `async/await` 避免阻塞 UI 线程：
  ```csharp
  private async void btnLoad_Click(object sender, EventArgs e) {
      try {
          var data = await _service.LoadDataAsync();
          dataGridView1.DataSource = data;
      } catch (Exception ex) {
          MessageBox.Show(ex.Message);
      }
  }

### **通义灵码生成要求**
```markdown
## AI 生成规则
- **语言**: 回答、代码注释使用中文
- **代码风格**:  
  - 生成的代码必须包含异常处理和资源释放  
  - 禁止使用 `Application.DoEvents()`（易导致性能问题）
- **控件推荐**: 优先使用官方控件（如 `DataGridView` 而非第三方网格控件）