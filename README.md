```c++
struct Mayuto {
    // Basic info
    std::string name = "Gáspár Mátyás";
    int age = 20;
    
    // Job info
    JobTitle job = JobTitle::SoftwareEngineer;
    std::string workplace = "Emerson Process Management";

    // Skills
    std::vector<Skill> skills = {
        {"C/C++", Level::Expert},
        {"JavaScript", Level::Expert},
        {"Java", Level::Expert},
        {"C#", Level:Intermediate},
        {"Rust", Level:Intermediate},
        {"Python", Level::Intermediate}
    };

    // Projects
    std::array<Project, 3> projects = {
        {"ColorSync", "Social & Drawing Platform for Artists", Status::Completed},
        {"IgnisUI", "Cross-platform UI Library in C/C++", Status::Active},
        {"Gup", "Smart Desktop Assistant", Status::Refactoring} // actively rewriting
    };

    // Other
    std::vector<std::string> hobbies = {"Robotics", "Music", "Low level coding"};
};
```
<p style="display:flex; gap:2%;">
  <img src="https://github-readme-stats.vercel.app/api?username=TanyasiPills&show_icons=true&theme=radical" style="width:68%; height:auto;" />
  <img src="https://github-readme-stats.vercel.app/api/top-langs/?username=TanyasiPills&theme=radical" style="width:30%; height:auto;" />
</p>
