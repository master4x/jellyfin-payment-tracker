const STATUS = {
    Paid: { label: '🟢 Vorausbezahlt' },
    Due: { label: '🟩 Fällig' },
    Overdue: { label: '🔴 Rückstand' },
    Grace: { label: '🎁 Kulanz' }
};

const STYLESHEET_ID = 'paymentTrackerCss';

const monthFormatter = new Intl.DateTimeFormat('de-DE', { month: 'long', year: 'numeric' });

function formatMonth(year, month) {
    return monthFormatter.format(new Date(year, month - 1, 1));
}

function toMonthInputValue(year, month) {
    return `${year}-${String(month).padStart(2, '0')}`;
}

function api(path) {
    return ApiClient.getUrl('PaymentTracker/' + path);
}

function post(path, body) {
    return ApiClient.ajax({
        type: 'POST',
        url: api(path),
        data: JSON.stringify(body),
        contentType: 'application/json',
        dataType: 'json'
    });
}

function el(tag, props, ...children) {
    const node = document.createElement(tag);
    Object.assign(node, props);
    for (const child of children) {
        node.append(child);
    }
    return node;
}

// Jellyfin only inserts the page container into the DOM, so the stylesheet
// (a separate embedded resource) is appended to <head> once.
function ensureStylesheet() {
    if (document.getElementById(STYLESHEET_ID)) {
        return;
    }
    document.head.append(el('link', {
        id: STYLESHEET_ID,
        rel: 'stylesheet',
        href: ApiClient.getUrl('web/ConfigurationPage', { name: 'PaymentTrackerCss' })
    }));
}

function button(label, onClick) {
    const btn = el('button', { type: 'button', className: 'raised emby-button' }, label);
    btn.setAttribute('is', 'emby-button');
    btn.addEventListener('click', onClick);
    return btn;
}

export default function (view) {
    ensureStylesheet();

    const tbody = view.querySelector('#ptBody');
    const summary = view.querySelector('#ptSummary');
    let users = [];

    function showError(err) {
        console.error('PaymentTracker', err);
        Dashboard.alert({ title: 'Payment Tracker', message: 'Aktion fehlgeschlagen. Details in der Browser-Konsole.' });
    }

    function renderSummary() {
        const counts = { Paid: 0, Due: 0, Overdue: 0, Grace: 0 };
        users.forEach(u => counts[u.Status]++);
        summary.replaceChildren(
            ...Object.keys(STATUS).map(key =>
                el('span', { className: 'pt-chip' }, `${STATUS[key].label}: ${counts[key]}`))
        );
    }

    function renderRow(user) {
        const tr = el('tr');
        tr.dataset.userId = user.UserId;

        // Status
        const status = el('span', { className: 'pt-status' }, STATUS[user.Status]?.label ?? user.Status);

        // Paid until – editable manually (SetMonth)
        const monthInput = el('input', {
            type: 'month',
            className: 'emby-input pt-month',
            value: toMonthInputValue(user.PaidUntilYear, user.PaidUntilMonth),
            title: formatMonth(user.PaidUntilYear, user.PaidUntilMonth)
        });
        monthInput.addEventListener('change', () => {
            const [y, m] = monthInput.value.split('-').map(Number);
            if (!y || !m) {
                return;
            }
            mutate(user, 'SetMonth', { Year: y, Month: m });
        });

        // Note – saved on Enter or when the field loses focus
        const saved = el('span', { className: 'pt-saved' }, '✓');
        const notesInput = el('input', {
            type: 'text',
            className: 'emby-input pt-notes',
            value: user.Notes ?? '',
            placeholder: 'Notiz …',
            maxLength: 1000
        });
        const saveNotes = () => {
            if (notesInput.value.trim() === (user.Notes ?? '')) {
                return;
            }
            post('UpdateNotes', { UserId: user.UserId, Notes: notesInput.value })
                .then(updated => {
                    Object.assign(user, updated);
                    saved.classList.add('show');
                    setTimeout(() => saved.classList.remove('show'), 1500);
                })
                .catch(showError);
        };
        notesInput.addEventListener('keydown', e => {
            if (e.key === 'Enter') {
                notesInput.blur();
            }
        });
        notesInput.addEventListener('blur', saveNotes);

        // Actions
        const actions = el('div', { className: 'pt-actions' },
            button('+1 Monat', () => mutate(user, 'AddMonth')),
            button(user.IsInGracePeriod ? 'Kulanz aus' : 'Kulanz an', () => mutate(user, 'ToggleGrace'))
        );

        tr.append(
            el('td', { className: 'pt-user' }, user.UserName),
            el('td', {}, status),
            el('td', {}, monthInput),
            el('td', {}, el('div', { className: 'pt-notes-cell' }, notesInput, saved)),
            el('td', {}, actions)
        );
        return tr;
    }

    function render() {
        if (!users.length) {
            tbody.replaceChildren(el('tr', {}, el('td', { colSpan: 5, className: 'pt-empty' }, 'Keine Benutzer gefunden.')));
        } else {
            tbody.replaceChildren(...users.map(renderRow));
        }
        renderSummary();
    }

    function mutate(user, endpoint, extra) {
        const row = tbody.querySelector(`tr[data-user-id="${user.UserId}"]`);
        row?.querySelectorAll('button, input').forEach(n => { n.disabled = true; });
        return post(endpoint, { UserId: user.UserId, ...extra })
            .then(updated => {
                const index = users.findIndex(u => u.UserId === updated.UserId);
                users[index] = updated;
                row?.replaceWith(renderRow(updated));
                renderSummary();
            })
            .catch(err => {
                showError(err);
                load();
            });
    }

    function load() {
        Dashboard.showLoadingMsg();
        return ApiClient.getJSON(api('Users'))
            .then(result => {
                users = result;
                render();
            })
            .catch(showError)
            .finally(() => Dashboard.hideLoadingMsg());
    }

    const now = new Date();
    view.querySelector('#ptCurrentMonth').textContent = formatMonth(now.getFullYear(), now.getMonth() + 1);
    view.querySelector('#ptRefresh').addEventListener('click', load);
    view.addEventListener('viewshow', load);
}
