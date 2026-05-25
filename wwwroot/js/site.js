(function () {
    const taskInput = document.getElementById('taskInput');
    const addBtn = document.getElementById('addBtn');
    const taskList = document.getElementById('taskList');
    const taskCount = document.getElementById('taskCount');
    const clearBtn = document.getElementById('clearBtn');

    let tasks = loadTasks();

    function loadTasks() {
        try {
            return JSON.parse(localStorage.getItem('todoTasks')) || [];
        } catch {
            return [];
        }
    }

    function saveTasks() {
        localStorage.setItem('todoTasks', JSON.stringify(tasks));
    }

    function render() {
        taskList.innerHTML = '';

        tasks.forEach((task, index) => {
            const li = document.createElement('li');
            li.className = 'task-item' + (task.completed ? ' completed' : '');

            const checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.className = 'task-checkbox';
            checkbox.checked = task.completed;
            checkbox.addEventListener('change', function () {
                tasks[index].completed = this.checked;
                saveTasks();
                render();
            });

            const span = document.createElement('span');
            span.className = 'task-text';
            span.textContent = task.text;

            const delBtn = document.createElement('button');
            delBtn.className = 'task-delete';
            delBtn.innerHTML = '\u00d7';
            delBtn.setAttribute('aria-label', 'Delete task');
            delBtn.addEventListener('click', function () {
                tasks.splice(index, 1);
                saveTasks();
                render();
            });

            li.appendChild(checkbox);
            li.appendChild(span);
            li.appendChild(delBtn);
            taskList.appendChild(li);
        });

        const total = tasks.length;
        const done = tasks.filter(t => t.completed).length;
        taskCount.textContent = total === 0 ? '0 tasks' : `${done}/${total} done`;
    }

    function addTask() {
        const text = taskInput.value.trim();
        if (!text) return;

        tasks.push({ text, completed: false });
        saveTasks();
        render();
        taskInput.value = '';
        taskInput.focus();
    }

    addBtn.addEventListener('click', addTask);

    taskInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') addTask();
    });

    clearBtn.addEventListener('click', function () {
        tasks = tasks.filter(t => !t.completed);
        saveTasks();
        render();
    });

    render();
})();
