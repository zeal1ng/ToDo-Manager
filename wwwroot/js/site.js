(function () {
    const taskInput = document.getElementById('taskInput');
    const addBtn = document.getElementById('addBtn');
    const taskList = document.getElementById('taskList');
    const taskCount = document.getElementById('taskCount');
    const clearBtn = document.getElementById('clearBtn');
    const sidebar = document.getElementById('sidebar');
    const pageHeading = document.getElementById('pageHeading');
    const myUserBtn = document.getElementById('myUserBtn');
    const userList = document.getElementById('userList');

    const user = window.__user;
    let tasks = [];
    let selectedUserId = null;
    let users = [];
    let chart = null;
    let chartDays = 7;
    const chartCanvas = document.getElementById('statsChart');

    async function loadTasks() {
        const url = selectedUserId
            ? '/api/usertask/user/' + selectedUserId
            : '/api/usertask';
        try {
            const res = await fetch(url, { credentials: 'include' });
            if (!res.ok) { tasks = []; render(); return; }
            tasks = await res.json();
        } catch {
            tasks = [];
        }
        render();
        loadStats();
    }

    async function loadUsers() {
        if (user.role !== 'Admin') return;
        try {
            const res = await fetch('/api/admin/users', { credentials: 'include' });
            if (!res.ok) return;
            users = await res.json();
            renderUserList();
        } catch {}
    }

    async function addTask() {
        const text = taskInput.value.trim();
        if (!text) return;

        const body = { title: text, body: '' };
        if (user.role === 'Admin' && selectedUserId)
            body.userId = selectedUserId;

        try {
            const res = await fetch('/api/usertask/create', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(body)
            });
            if (!res.ok) return;
            const task = await res.json();
            tasks.unshift(task);
            render();
            taskInput.value = '';
            taskInput.focus();
            loadSidebarStats();
        } catch {}
    }

    async function toggleTask(id, completed) {
        try {
            await fetch('/api/usertask/' + id, {
                method: 'PUT',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ status: completed ? 'Completed' : 'Incompleted' })
            });
        } catch {}
    }

    async function updateTaskBody(id, body) {
        try {
            await fetch('/api/usertask/' + id, {
                method: 'PUT',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ body: body || '' })
            });
        } catch {}
    }

    async function deleteTask(id) {
        try {
            await fetch('/api/usertask/' + id, {
                method: 'DELETE',
                credentials: 'include'
            });
        } catch {}
    }

    async function clearCompleted() {
        let url = '/api/usertask/clear-completed';
        if (user.role === 'Admin' && selectedUserId)
            url += '?userId=' + selectedUserId;

        try {
            await fetch(url, {
                method: 'DELETE',
                credentials: 'include'
            });
            tasks = tasks.filter(t => t.status !== 'Completed');
            render();
            loadSidebarStats();
        } catch {}
    }

    async function loadStats() {
        if (!chartCanvas) return;
        var url = '/api/usertask/stats?days=' + chartDays;
        if (user.role === 'Admin' && selectedUserId)
            url += '&userId=' + selectedUserId;
        try {
            var res = await fetch(url, { credentials: 'include' });
            if (!res.ok) return;
            var data = await res.json();
            renderChart(data);
        } catch {}
    }

    function renderChart(data) {
        var labels = data.map(function (d) { return d.date.slice(5); });
        var created = data.map(function (d) { return d.created; });
        var completed = data.map(function (d) { return d.completed; });
        var totalCreated = created.reduce(function (a, b) { return a + b; }, 0);
        var totalCompleted = completed.reduce(function (a, b) { return a + b; }, 0);

        if (chart) chart.destroy();
        chart = new Chart(chartCanvas, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: 'Created',
                        data: created,
                        backgroundColor: 'rgba(108, 92, 231, 0.7)',
                        borderColor: 'rgba(108, 92, 231, 1)',
                        borderWidth: 1,
                        borderRadius: 4
                    },
                    {
                        label: 'Completed',
                        data: completed,
                        backgroundColor: 'rgba(0, 204, 150, 0.7)',
                        borderColor: 'rgba(0, 204, 150, 1)',
                        borderWidth: 1,
                        borderRadius: 4
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 10 } }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: {
                            stepSize: 1,
                            font: { size: 10 }
                        },
                        grid: { color: 'rgba(0,0,0,0.06)' }
                    }
                }
            }
        });

        document.getElementById('statCreated').textContent = totalCreated;
        document.getElementById('statCompleted').textContent = totalCompleted;
    }

    async function deleteUser(userId) {
        if (!confirm('Delete this user and all their tasks?')) return;
        try {
            const res = await fetch('/api/admin/users/' + userId, {
                method: 'DELETE',
                credentials: 'include'
            });
            if (!res.ok) return;
            if (selectedUserId === userId) {
                selectedUserId = null;
                pageHeading.textContent = 'My Tasks';
                loadTasks();
            }
            loadUsers();
            loadSidebarStats();
        } catch {}
    }

    function addUser() {
        const name = prompt('Username:');
        if (!name) return;
        const password = prompt('Password:');
        if (!password) return;
        const isAdmin = confirm('Make this user an Admin?\nOK = Admin, Cancel = regular user.');

        fetch('/api/admin/users', {
            method: 'POST',
            credentials: 'include',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                userName: name,
                password: password,
                userRole: isAdmin ? 0 : 1
            })
        })
            .then(function (r) { return r.ok ? r.json() : r.json().then(function (d) { throw new Error(d.message); }); })
            .then(function () { loadUsers(); })
            .catch(function (e) { alert(e.message || 'Failed to create user'); });
    }

    function selectUser(userId) {
        selectedUserId = userId;
        var u = users.find(function (x) { return x.id === userId; });
        pageHeading.textContent = u ? u.userName + "'s Tasks" : 'My Tasks';
        loadTasks();
        renderUserList();
    }

    function selectMyTasks() {
        selectedUserId = null;
        pageHeading.textContent = 'My Tasks';
        loadTasks();
        var allUsers = document.querySelectorAll('.sidebar-user');
        allUsers.forEach(function (el) { el.classList.remove('active'); });
        if (myUserBtn) myUserBtn.classList.add('active');
    }

    function renderUserList() {
        if (!userList) return;
        var filtered = users.filter(function (u) { return u.id !== user.id; });
        userList.innerHTML = filtered.map(function (u) {
            return '<div class="sidebar-user' + (selectedUserId === u.id ? ' active' : '') + '" data-user-id="' + u.id + '">' +
                '<div class="sidebar-user-info" data-view="' + u.id + '">' +
                    '<div class="sidebar-user-name">' + escapeHtml(u.userName) + '</div>' +
                    '<div class="sidebar-user-stats">' + u.completedTasks + '/' + u.totalTasks + ' done</div>' +
                '</div>' +
                '<button class="btn-icon btn-icon-danger" data-delete="' + u.id + '" title="Delete user">&times;</button>' +
            '</div>';
        }).join('');

        userList.querySelectorAll('[data-view]').forEach(function (el) {
            el.addEventListener('click', function () {
                selectUser(parseInt(this.dataset.view));
            });
        });

        userList.querySelectorAll('[data-delete]').forEach(function (el) {
            el.addEventListener('click', function (e) {
                e.stopPropagation();
                deleteUser(parseInt(this.dataset.delete));
            });
        });

        highlightSelected();
    }

    function highlightSelected() {
        var allUsers = document.querySelectorAll('.sidebar-user');
        allUsers.forEach(function (el) { el.classList.remove('active'); });
        if (selectedUserId === null) {
            if (myUserBtn) myUserBtn.classList.add('active');
        } else {
            var el = document.querySelector('.sidebar-user[data-user-id="' + selectedUserId + '"]');
            if (el) el.classList.add('active');
        }
    }

    function escapeHtml(str) {
        var div = document.createElement('div');
        div.textContent = str;
        return div.innerHTML;
    }

    function render() {
        taskList.innerHTML = '';

        tasks.forEach(function (task) {
            var completed = task.status === 'Completed';

            var li = document.createElement('li');
            li.className = 'task-item' + (completed ? ' completed' : '');

            var checkbox = document.createElement('input');
            checkbox.type = 'checkbox';
            checkbox.className = 'task-checkbox';
            checkbox.checked = completed;
            checkbox.addEventListener('change', async function () {
                task.status = this.checked ? 'Completed' : 'Incompleted';
                await toggleTask(task.id, this.checked);
                render();
            });

            var content = document.createElement('div');
            content.className = 'task-content';

            var titleRow = document.createElement('div');
            titleRow.className = 'task-title-row';

            var span = document.createElement('span');
            span.className = 'task-text';
            span.textContent = task.title;

            var expandBtn = document.createElement('button');
            expandBtn.className = 'task-expand-btn';
            expandBtn.textContent = '\u25B8';
            expandBtn.setAttribute('aria-label', 'Toggle description');
            expandBtn.addEventListener('click', function () {
                var isHidden = bodyArea.style.display === 'none';
                bodyArea.style.display = isHidden ? 'block' : 'none';
                this.textContent = isHidden ? '\u25BE' : '\u25B8';
            });

            titleRow.appendChild(span);
            titleRow.appendChild(expandBtn);

            var bodyArea = document.createElement('div');
            bodyArea.className = 'task-body-area';
            bodyArea.style.display = 'none';

            var bodyText = document.createElement('div');
            bodyText.className = 'task-body-text';
            bodyText.textContent = task.body || 'No description';
            bodyText.contentEditable = false;

            var bodyEdit = document.createElement('textarea');
            bodyEdit.className = 'task-body-edit';
            bodyEdit.value = task.body || '';
            bodyEdit.placeholder = 'Add a description...';

            var bodyActions = document.createElement('div');
            bodyActions.className = 'task-body-actions';

            var editBtn = document.createElement('button');
            editBtn.className = 'btn btn-small btn-secondary';
            editBtn.textContent = 'Edit';
            editBtn.addEventListener('click', function () {
                bodyEdit.value = task.body || '';
                bodyText.style.display = 'none';
                bodyEdit.style.display = 'block';
                editBtn.style.display = 'none';
                saveBtn.style.display = 'inline-block';
                cancelBtn.style.display = 'inline-block';
                bodyEdit.focus();
            });

            var saveBtn = document.createElement('button');
            saveBtn.className = 'btn btn-small btn-primary';
            saveBtn.textContent = 'Save';
            saveBtn.style.display = 'none';
            saveBtn.addEventListener('click', async function () {
                var newBody = bodyEdit.value;
                await updateTaskBody(task.id, newBody);
                task.body = newBody;
                bodyText.textContent = newBody || 'No description';
                bodyText.style.display = 'block';
                bodyEdit.style.display = 'none';
                editBtn.style.display = 'inline-block';
                saveBtn.style.display = 'none';
                cancelBtn.style.display = 'none';
                loadSidebarStats();
            });

            var cancelBtn = document.createElement('button');
            cancelBtn.className = 'btn btn-small btn-secondary';
            cancelBtn.textContent = 'Cancel';
            cancelBtn.style.display = 'none';
            cancelBtn.addEventListener('click', function () {
                bodyEdit.value = task.body || '';
                bodyText.style.display = 'block';
                bodyEdit.style.display = 'none';
                editBtn.style.display = 'inline-block';
                saveBtn.style.display = 'none';
                cancelBtn.style.display = 'none';
            });

            bodyActions.appendChild(editBtn);
            bodyActions.appendChild(saveBtn);
            bodyActions.appendChild(cancelBtn);

            bodyArea.appendChild(bodyText);
            bodyArea.appendChild(bodyEdit);
            bodyArea.appendChild(bodyActions);

            content.appendChild(titleRow);
            content.appendChild(bodyArea);

            var delBtn = document.createElement('button');
            delBtn.className = 'task-delete';
            delBtn.innerHTML = '\u00d7';
            delBtn.setAttribute('aria-label', 'Delete task');
            delBtn.addEventListener('click', async function () {
                tasks = tasks.filter(function (t) { return t.id !== task.id; });
                await deleteTask(task.id);
                render();
            });

            li.appendChild(checkbox);
            li.appendChild(content);
            li.appendChild(delBtn);
            taskList.appendChild(li);
        });

        var total = tasks.length;
        var done = tasks.filter(function (t) { return t.status === 'Completed'; }).length;
        taskCount.textContent = total === 0 ? '0 tasks' : done + '/' + total + ' done';
        loadSidebarStats();
        loadStats();
    }

    function loadSidebarStats() {
        if (!selectedUserId) {
            var total = tasks.length;
            var done = tasks.filter(function (t) { return t.status === 'Completed'; }).length;
            var statsEl = document.getElementById('myStats');
            if (statsEl) statsEl.textContent = done + '/' + total + ' done';
        }
        if (user.role === 'Admin') loadUsers();
    }

    if (myUserBtn) {
        myUserBtn.addEventListener('click', selectMyTasks);
    }

    var addUserBtn = document.getElementById('addUserBtn');
    if (addUserBtn) {
        addUserBtn.addEventListener('click', addUser);
    }

    addBtn.addEventListener('click', addTask);

    taskInput.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') addTask();
    });

    clearBtn.addEventListener('click', clearCompleted);

    document.querySelectorAll('.range-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            document.querySelectorAll('.range-btn').forEach(function (b) { b.classList.remove('active'); });
            this.classList.add('active');
            chartDays = parseInt(this.dataset.days);
            loadStats();
        });
    });

    selectMyTasks();
    if (user.role === 'Admin') loadUsers();
})();
