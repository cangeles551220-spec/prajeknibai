const roles = {
  ADMIN: {
    initials: 'AD',
    profile: { name: 'Alex Morgan', role: 'Administrator' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'tracking', 'customers', 'devices', 'technicians', 'inventory', 'suppliers', 'billing', 'crm', 'reports', 'settings']
  },
  MANAGER: {
    initials: 'MG',
    profile: { name: 'Maya Lopez', role: 'Manager' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'tracking', 'customers', 'devices', 'technicians', 'inventory', 'suppliers', 'billing', 'crm', 'reports']
  },
  TECHNICIAN: {
    initials: 'TC',
    profile: { name: 'Noah Williams', role: 'Technician' },
    defaultView: 'repairs',
    nav: ['repairs', 'tracking']
  },
  STAFF: {
    initials: 'ST',
    profile: { name: 'Sheilo', role: 'Employee / Service Staff' },
    defaultView: 'customers',
    nav: ['overview', 'customers', 'devices', 'repairs', 'tracking', 'billing', 'crm']
  },
  INVENTORY: {
    initials: 'IN',
    profile: { name: 'Riley Chen', role: 'Inventory' },
    defaultView: 'inventory',
    nav: ['overview', 'inventory', 'suppliers', 'devices', 'repairs', 'reports']
  },
  BILLING: {
    initials: 'BL',
    profile: { name: 'Sofia Patel', role: 'Billing Staff' },
    defaultView: 'billing',
    nav: ['overview', 'billing', 'customers', 'repairs', 'crm', 'reports']
  },
  BILLING: {
    initials: 'BL',
    profile: { name: 'Sofia Patel', role: 'Billing' },
    defaultView: 'billing',
    nav: ['overview', 'billing', 'customers', 'repairs', 'crm', 'reports']
  }
};

const state = {
  view: 'overview',
  currentUserRole: 'ADMIN',
  jobs: [
    {id:'JOB-1048', customer:'Priya Nair', initials:'PN', device:'Dell XPS 15', issue:'Laptop won’t power on', tech:'Noah Williams', status:'In Repair', priority:'High', received:'Sep 08, 2024', due:'Sep 10, 2024', progress:64},
    {id:'JOB-1047', customer:'Marcus Reed', initials:'MR', device:'MacBook Pro 14"', issue:'Screen replacement', tech:'Sofia Patel', status:'Testing', priority:'Normal', received:'Sep 08, 2024', due:'Sep 09, 2024', progress:86},
    {id:'JOB-1046', customer:'Elena Cruz', initials:'EC', device:'HP EliteBook 840', issue:'Slow performance / cleanup', tech:'Noah Williams', status:'Waiting for Parts', priority:'Normal', received:'Sep 07, 2024', due:'Sep 11, 2024', progress:42},
    {id:'JOB-1045', customer:'Jordan Lee', initials:'JL', device:'Lenovo ThinkPad T14', issue:'Keyboard not responding', tech:'Liam Chen', status:'Diagnosing', priority:'Urgent', received:'Sep 07, 2024', due:'Sep 09, 2024', progress:23},
    {id:'JOB-1044', customer:'Maya Thompson', initials:'MT', device:'Custom Desktop', issue:'Random restarts', tech:'Sofia Patel', status:'Completed', priority:'Normal', received:'Sep 06, 2024', due:'Sep 08, 2024', progress:100},
  ],
  customers: [
    {id:'CUS-0204',name:'Priya Nair',contact:'+1 (415) 555-0182',email:'priya.nair@email.com',jobs:4,status:'Returning'},
    {id:'CUS-0203',name:'Marcus Reed',contact:'+1 (312) 555-0144',email:'marcus.reed@email.com',jobs:2,status:'Active'},
    {id:'CUS-0202',name:'Elena Cruz',contact:'+1 (206) 555-0196',email:'elena.cruz@email.com',jobs:1,status:'Active'},
    {id:'CUS-0201',name:'Jordan Lee',contact:'+1 (646) 555-0128',email:'jordan.lee@email.com',jobs:3,status:'Returning'},
  ],
  parts: [
    {id:'PRT-0031',name:'Dell 65W USB-C adapter',category:'Power',stock:4,reorder:5,cost:'$28.00',supplier:'TechSource Supply'},
    {id:'PRT-0028',name:'Samsung 1TB NVMe SSD',category:'Storage',stock:12,reorder:4,cost:'$64.00',supplier:'PartsHub'},
    {id:'PRT-0024',name:'MacBook Pro A2442 display',category:'Display',stock:2,reorder:3,cost:'$310.00',supplier:'Apple Parts Co.'},
    {id:'PRT-0021',name:'DDR4 16GB SODIMM',category:'Memory',stock:0,reorder:5,cost:'$42.00',supplier:'PartsHub'},
  ],
  devices: [],
  devicesLoaded: false,
  invoices: [
    { invoiceId: 1, id:'INV-0001',job:'JOB-0001',customer:'Maria Santos',amount:'₱3,500.00',paid:'₱0.00',status:'Unpaid',date:'Sep 08, 2024'},
    { invoiceId: 2, id:'INV-0002',job:'JOB-0002',customer:'Jose Reyes',amount:'₱1,800.00',paid:'₱0.00',status:'Unpaid',date:'Sep 08, 2024'},
    { invoiceId: 3, id:'INV-0003',job:'JOB-0003',customer:'Ahn Villanueva',amount:'₱6,500.00',paid:'₱0.00',status:'Unpaid',date:'Sep 07, 2024'},
  ],
  assignmentJobId: null,
  parts: [],
  partsLoaded: false,
  crmCustomerIndex: 0,
  trackingJobs: null,
  trackingLoaded: false,
  techniciansLoaded: false,
  assignableTechnicians: [],
  customersLoaded: false,
  suppliersLoaded: false,
  billingLoaded: false,
  reportSummary: null,
  reportsLoaded: false,
  suppliers: [],
  technicians: []
};

const content = document.getElementById('content');
const serverUser = document.getElementById('appShell')?.dataset;
if (serverUser?.role && roles[serverUser.role]) {
  const pictureKey = `techserve_profile_picture_${serverUser.username || serverUser.role}`;
  window.__TECHSERVE_USER__ = {
    role: serverUser.role,
    name: serverUser.name || 'User',
    username: serverUser.username || serverUser.role,
    picture: localStorage.getItem(pictureKey) || '',
    workspace: serverUser.workspace || 'TechServe HQ'
  };
}
const statusClass = s => ({
  'Completed':'status-green',
  'Paid':'status-green',
  'In Repair':'status-blue',
  'Testing':'status-blue',
  'Diagnosing':'status-orange',
  'Waiting for Parts':'status-orange',
  'Pending':'status-gray',
  'Unpaid':'status-red',
  'Partially Paid':'status-orange',
  'Released':'status-green',
  'Ready for Pickup':'status-green',
  'Cancelled':'status-red',
  'Returning':'status-green',
  'Active':'status-blue'
}[s] || 'status-gray');

const parseMoney = value => Number(String(value).replace(/[₱$,]/g, '').trim()) || 0;
const resolveInvoiceId = invoice => Number(invoice?.invoiceId ?? invoice?.id?.match(/\d+/)?.[0] ?? 0) || 0;
const money = value => `₱${Number(value).toLocaleString('en-PH',{minimumFractionDigits:2,maximumFractionDigits:2})}`;
const initials = name => name.split(' ').map(v=>v[0]).join('').slice(0,2);
const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character]));
const userProfile = () => ({
  name: window.__TECHSERVE_USER__?.name || 'User',
  role: roleLabel(state.currentUserRole)
});
const applyAppearance = () => {
  const appearance = localStorage.getItem('techserve_appearance') || 'light';
  document.body.dataset.appearance = appearance;
  const appShell = document.getElementById('appShell');
  if (appShell) appShell.dataset.appearance = appearance;
};
applyAppearance();
const timeGreeting = () => {
  const hour = new Date().getHours();
  if (hour < 12) return 'Good morning';
  if (hour < 18) return 'Good afternoon';
  return 'Good evening';
};
const roleLabel = role => ({ ADMIN: 'Administrator', MANAGER: 'Manager', TECHNICIAN: 'Technician', STAFF: 'Cashier / Staff', INVENTORY: 'Inventory Staff', BILLING: 'Cashier / Staff' }[String(role || '').toUpperCase()] || 'User');
const roleColorClass = role => ({ ADMIN: 'role-orange', MANAGER: 'role-orange', TECHNICIAN: 'role-blue', STAFF: 'role-green', INVENTORY: 'role-blue', BILLING: 'role-green' }[String(role || '').toUpperCase()] || 'role-neutral');
function initialViewFor(role) {
  const requestedView = new URLSearchParams(window.location.search).get('view');
  const specialViews = new Set(['profile']);
  if (requestedView && (specialViews.has(requestedView) || roles[role]?.nav.includes(requestedView))) {
    return requestedView;
  }
  return roles[role]?.defaultView || 'overview';
}

function layout(title, subtitle, action = '') {
  return `<div class="page-heading"><div><p class="eyebrow">TechServe workspace</p><h1>${title}</h1><p>${subtitle}</p></div>${action}</div>`;
}
function person(name, code = initials(name)) { return `<span class="person"><span class="mini-avatar">${code}</span>${name}</span>`; }
function status(s) { return `<span class="status ${statusClass(s)}">${s}</span>`; }

function applyRoleProfile() {
  const role = roles[state.currentUserRole] || roles.ADMIN;
  const profile = role.profile;
  const colorClass = roleColorClass(state.currentUserRole);
  const wsInitials = document.getElementById('workspaceInitials');
  if (wsInitials) wsInitials.textContent = role.initials;
  const wsRole = document.getElementById('workspaceRole');
  if (wsRole) { wsRole.textContent = roleLabel(state.currentUserRole); wsRole.className = `role-label ${colorClass}`; }
  const profInitials = document.getElementById('profileInitials');
  if (profInitials) {
    const picture = window.__TECHSERVE_USER__?.picture || '';
    profInitials.innerHTML = picture
      ? `<img src="${escapeHtml(picture)}" alt="Profile picture" style="width:100%;height:100%;object-fit:cover;border-radius:inherit;">`
      : (window.__TECHSERVE_USER__?.name || profile.name).split(' ').map(x => x[0]).join('').slice(0,2).toUpperCase();
  }
  const profName = document.getElementById('profileName');
  if (profName) profName.textContent = window.__TECHSERVE_USER__?.name || profile.name;
  const profRole = document.getElementById('profileRole');
  if (profRole) { profRole.textContent = roleLabel(state.currentUserRole); profRole.className = `role-label ${colorClass}`; }
}

function applyNavigationAccess() {
  const allowed = new Set(roles[state.currentUserRole].nav);
  const specialViews = new Set(['profile']);
  document.querySelectorAll('.nav-item[data-view]').forEach((item) => {
    item.style.display = allowed.has(item.dataset.view) ? '' : 'none';
    item.classList.toggle('active', item.dataset.view === state.view);
  });
  const overviewLink = document.querySelector('.nav-item[href="/Admin/Dashboard"]');
  if (overviewLink) overviewLink.style.display = allowed.has('overview') ? '' : 'none';

  const currentRoleViews = roles[state.currentUserRole].nav;
  if (!currentRoleViews.includes(state.view) && !specialViews.has(state.view)) {
    state.view = currentRoleViews[0];
  }
}

function renderOverview() {
  const jobRows = state.jobs.slice(0,4).map(j => `<tr><td><b>${j.id}</b></td><td>${person(j.customer,j.initials)}</td><td>${j.device}</td><td>${j.tech}</td><td>${status(j.status)}</td><td><span class="tag">${j.priority}</span></td></tr>`).join('');
  const activeJobs = state.jobs.filter(job => job.status !== 'Completed' && job.status !== 'Cancelled').length;
  const completedRepairs = state.jobs.filter(job => job.status === 'Completed').length;
  const totalRevenue = state.invoices.reduce((total, invoice) => total + parseMoney(invoice.amount), 0);
  const totalCustomers = state.customers.length || 0;
  return layout(timeGreeting() + ', ' + userProfile().name.split(' ')[0] + ' 👋','Here’s what’s happening at your repair shop today.') +
    `<div class="stat-grid">
      <div class="stat-card"><div class="stat-top">Total customers <span class="stat-icon i-blue">♙</span></div><h2>${totalCustomers}</h2><span class="trend">Live records</span></div>
      <div class="stat-card"><div class="stat-top">Active repair jobs <span class="stat-icon i-orange">▣</span></div><h2>${activeJobs}</h2><span class="trend">Live status</span></div>
      <div class="stat-card"><div class="stat-top">Completed repairs <span class="stat-icon i-green">✓</span></div><h2>${completedRepairs}</h2><span class="trend">Saved repairs</span></div>
      <div class="stat-card"><div class="stat-top">Total revenue <span class="stat-icon i-blue">$</span></div><h2>${money(totalRevenue)}</h2><span class="trend">Invoice total</span></div>
    </div>
    <div class="dashboard-grid">
      <section class="panel"><div class="panel-header"><div><h3>Revenue overview</h3><p>Monthly revenue from repair jobs and parts</p></div><div class="chart-legend"><span class="legend"><i class="dot" style="background:#3268e8"></i>Revenue</span><span class="legend"><i class="dot" style="background:#9eb9f8"></i>Last year</span><select class="select" style="height:28px;font-size:10px"><option>Last 6 months</option></select></div></div>
        <div class="chart"><div class="chart-grid"><span><em>$30k</em></span><span><em>$20k</em></span><span><em>$10k</em></span><span><em>$0</em></span></div><svg viewBox="0 0 600 170" preserveAspectRatio="none"><path d="M0 140 C45 130 60 110 100 119 S160 93 200 101 S250 67 300 79 S350 48 400 57 S450 35 500 42 S550 13 600 23" fill="none" stroke="#3268e8" stroke-width="3"/><path d="M0 140 C45 130 60 110 100 119 S160 93 200 101 S250 67 300 79 S350 48 400 57 S450 35 500 42 S550 13 600 23 V170 H0Z" fill="#3268e8" opacity=".07"/><path d="M0 151 C45 143 60 138 100 145 S160 119 200 130 S250 110 300 115 S350 98 400 102 S450 84 500 93 S550 69 600 76" fill="none" stroke="#a9bff3" stroke-width="2" stroke-dasharray="4 4"/></svg><div class="x-axis"><span>Apr</span><span>May</span><span>Jun</span><span>Jul</span><span>Aug</span><span>Sep</span></div></div>
      </section>
      <section class="panel"><div class="panel-header"><div><h3>Repair job status</h3><p>${activeJobs} active jobs across the shop</p></div><button class="panel-link" data-view-link="repairs">View all</button></div><div class="donut-wrap"><div class="donut"></div><div class="legend-list"><span class="legend"><i class="dot" style="background:#3268e8"></i>In repair <b>${state.jobs.filter(job => job.status === 'In Repair').length}</b></span><span class="legend"><i class="dot" style="background:#54bd93"></i>Completed <b>${completedRepairs}</b></span><span class="legend"><i class="dot" style="background:#f4a95b"></i>Diagnosing <b>${state.jobs.filter(job => job.status === 'Diagnosis').length}</b></span><span class="legend"><i class="dot" style="background:#d6ddea"></i>Other <b>${Math.max(0, activeJobs - state.jobs.filter(job => ['In Repair', 'Diagnosis'].includes(job.status)).length)}</b></span></div></div></section>
    </div>
    <section class="panel recent"><div class="panel-header"><div><h3>Recent repair jobs</h3><p>Latest activity across your repair floor</p></div><button class="panel-link" data-view-link="repairs">View all jobs →</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Job order</th><th>Customer</th><th>Device</th><th>Technician</th><th>Status</th><th>Priority</th></tr></thead><tbody>${jobRows}</tbody></table></div></section>`;
}

function renderRepairs() {
  const isTechnician = state.currentUserRole === 'TECHNICIAN';
  const jobs = isTechnician ? (state.trackingLoaded ? state.jobs : []) : state.jobs;
  const canAssign = ['ADMIN', 'MANAGER'].includes(state.currentUserRole);
  const canCreate = ['ADMIN','MANAGER','STAFF'].includes(state.currentUserRole);
  const tabs = ['All', 'Received', 'Diagnosis', 'In Repair', 'Testing', 'Ready for Pickup', 'Completed', 'Cancelled'];
  const statusLabel = s => s;
  const priorityClass = p => p === 'Urgent' || p === 'High' ? 'priority-high' : p === 'Low' ? 'priority-low' : 'priority-medium';
  const jobRows = jobs.map(job => `<tr data-repair-status="${job.status}"><td><b class="repair-id">${escapeHtml(job.id)}</b></td><td>${escapeHtml(job.customer)}</td><td>${escapeHtml(job.device)}</td><td>${escapeHtml(job.issue)}</td><td>${escapeHtml(job.tech)}</td><td>${escapeHtml(job.due)}</td><td><span class="priority-badge ${priorityClass(job.priority)}">${escapeHtml(job.priority)}</span></td><td>${status(job.status)}</td><td><b>${money(job.amount || 0)}</b></td><td class="repair-actions"><button type="button" class="view-repair-details" data-repair-id="${job.repairJobId}" aria-label="View repair details" title="View repair details">◉</button>${canAssign && !['Completed', 'Cancelled'].includes(job.status) ? `<button type="button" class="assign-technician" data-job-id="${job.repairJobId}" aria-label="Assign technician" title="Assign technician">♧</button>` : ''}</td></tr>`).join('');
  const emptyMessage = isTechnician && state.trackingLoaded ? 'No repair jobs are currently assigned to your account.' : 'Loading assigned repair jobs...';
  return `<div class="repair-heading"><div><h1>Repair Jobs</h1><p>${isTechnician ? `${jobs.length} assigned repair jobs` : `${jobs.length} total job orders`}</p></div>${canCreate ? '<button class="button button-primary" id="newJob">＋ Create Repair Job</button>' : ''}</div>
    <div class="repair-tabs">${tabs.map((tab, index) => `<button class="repair-tab ${index === 0 ? 'active' : ''}" data-status-tab="${tab}">${tab}</button>`).join('')}</div>
    <section class="panel repair-search-panel"><input class="table-search" id="tableSearch" placeholder="⌕  Search job ID, customer, technician..."><button class="button button-ghost">▽ Filter</button></section>
    <section class="panel repair-table-panel"><div class="table-wrap"><table class="data-table repair-table" id="repairTable"><thead><tr><th>Job ID</th><th>Customer</th><th>Device</th><th>Problem</th><th>Technician</th><th>Expected</th><th>Priority</th><th>Status</th><th>Cost</th><th>Actions</th></tr></thead><tbody>${jobRows || `<tr><td colspan="10">${emptyMessage}</td></tr>`}</tbody></table></div></section>`;
}

function renderTracking() {
  const stages = ['Received', 'Diagnosis', 'In Repair', 'Testing', 'Ready for Pickup', 'Completed'];
  const fallbackJobs = [
    { id: 'RJ-2024-001', customer: 'Maria Santos', device: 'Lenovo ThinkPad E15', tech: 'Carlo Mendoza', due: '2024-11-22', received: '2024-11-18', amount: '₱3,500', current: 2 },
    { id: 'RJ-2024-002', customer: 'Jose Reyes', device: 'HP Pavilion TP01', tech: 'Diana Aquino', due: '2024-11-21', received: '2024-11-17', amount: '₱1,800', current: 4 },
    { id: 'RJ-2024-003', customer: 'Ahn Villanueva', device: 'MacBook Air M2', tech: 'Carlo Mendoza', due: '2024-11-20', received: '2024-11-15', amount: '₱6,500', current: 2 },
    { id: 'RJ-2024-004', customer: 'Liza Fernandez', device: 'Dell Inspiron 3891', tech: 'Ben Torres', due: '2024-11-23', received: '2024-11-19', amount: '₱2,200', current: 1 },
    { id: 'RJ-2024-005', customer: 'Rafael Cruz', device: 'ASUS VivoBook 15', tech: 'Diana Aquino', due: '2024-11-24', received: '2024-11-20', amount: '₱900', current: 0 },
    { id: 'RJ-2024-006', customer: 'Marco Dela Rosa', device: 'Dell XPS 15', tech: 'Ben Torres', due: '2024-11-19', received: '2024-11-16', amount: '₱8,500', current: 5 }
  ];
  fallbackJobs.forEach(job => {
    job.status = stages[job.current];
  });
  const trackingJobs = state.trackingJobs || (state.currentUserRole === 'TECHNICIAN' ? [] : fallbackJobs);
  const stageIndex = status => Math.max(0, stages.indexOf(status));
  return layout('Repair Status Tracking', 'Live progress tracking for all active repair jobs') +
    `<div class="tracking-list">${trackingJobs.length ? trackingJobs.map(job => `<article class="tracking-card" data-tracking-id="${job.repairJobId || job.id}" tabindex="0" role="button" aria-label="Open repair details for ${job.jobId || job.id}">
      <div class="tracking-header"><div><b class="tracking-id">${job.id}</b><strong>${job.customer}</strong><p>${job.device} · ${job.tech} · Expected: ${job.due}</p></div><div class="tracking-amount"><small>Received: ${job.received}</small><b>${job.amount}</b></div></div>
      <div class="tracking-timeline">${stages.map((stage, index) => `<div class="tracking-stage ${index < stageIndex(job.status) ? 'complete' : index === stageIndex(job.status) ? 'current' : ''}"><span>${index < stageIndex(job.status) ? '✓' : index === stageIndex(job.status) ? '◉' : '○'}</span><small>${stage}</small></div>`).join('')}</div>
    </article>`).join('') : `<section class="panel"><h3>${state.trackingLoaded ? 'No assigned repair jobs yet' : 'Loading assigned repair jobs...'}</h3><p>${state.trackingLoaded ? 'Repair jobs assigned to your account will appear here.' : 'Please wait while your assigned jobs load.'}</p></section>`}</div>`;
}

function renderCustomers() {
  const customerFilter = state.customerStatusFilter || 'Active';
  const visibleCustomers = state.customers.filter(customer => customerFilter === 'All customers'
    || customerFilter === customer.status
    || (customerFilter === 'Returning customers' && customer.status === 'Returning'));
  const customerRows = visibleCustomers.map(c => `<tr><td>${person(c.name)}</td><td>${c.id}</td><td>${c.contact}</td><td>${c.email}</td><td><b>${c.jobs}</b></td><td>${status(c.status)}</td><td><button type="button" class="panel-link customer-view-profile" data-customer-id="${c.customerId}">View profile</button>${c.status !== 'Archived' ? ` <button type="button" class="panel-link archive-customer" data-customer-id="${c.customerId}" title="Archive customer">Archive</button>` : ''}</td></tr>`).join('');
  return layout('Customers','Keep customer profiles, contact details and repair history in one place.','<button class="button button-primary" id="newCustomer">＋ Add customer</button>')+
    `<section class="panel"><div class="toolbar"><input class="table-search" id="tableSearch" placeholder="⌕  Search customers..."><select class="select" id="customerStatusFilter"><option ${customerFilter === 'Active' ? 'selected' : ''}>Active</option><option ${customerFilter === 'All customers' ? 'selected' : ''}>All customers</option><option ${customerFilter === 'Returning customers' ? 'selected' : ''}>Returning customers</option><option ${customerFilter === 'Archived' ? 'selected' : ''}>Archived</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Customer</th><th>Customer ID</th><th>Contact</th><th>Email</th><th>Repair jobs</th><th>Status</th><th>Actions</th></tr></thead><tbody>${customerRows || '<tr><td colspan="7">No customers match this filter.</td></tr>'}</tbody></table></div></section>`;
}

function renderDevices() {
  const canManage = ['ADMIN', 'MANAGER', 'STAFF'].includes(state.currentUserRole);
  const actions = canManage ? '<button class="button button-primary">＋ Register device</button>' : '';
  const rows = state.devices.map(device => `<tr><td><b>${escapeHtml(device.id)}</b></td><td>${person(escapeHtml(device.customer))}</td><td><b>${escapeHtml(device.device)}</b></td><td>${escapeHtml(device.serial || '—')}</td><td>${escapeHtml(device.operatingSystem || '—')}</td><td>${escapeHtml(device.condition || '—')}</td><td>${device.repairCount} repairs ${canManage ? `<button class="panel-link edit-device" data-device-id="${device.deviceId}">Edit</button>` : ''}</td></tr>`).join('');
  return layout('Devices','A complete view of every device registered with your shop.',actions)+`<section class="panel"><div class="toolbar"><input class="table-search" id="deviceSearch" placeholder="⌕  Search device, serial or customer..."><select class="select" id="deviceTypeFilter"><option>All device types</option>${[...new Set(state.devices.map(device=>device.deviceType))].filter(Boolean).map(type=>`<option>${escapeHtml(type)}</option>`).join('')}</select><span class="spacer"></span><button class="button button-ghost" id="exportDevices">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Device ID</th><th>Customer</th><th>Device</th><th>Serial number</th><th>Operating system</th><th>Condition</th><th>Repair history</th></tr></thead><tbody>${rows || `<tr><td colspan="7">${state.devicesLoaded ? 'No devices registered.' : 'Loading devices...'}</td></tr>`}</tbody></table></div></section>`;
}

function renderTechnicians() {
  return layout('Technicians', 'Manage technicians and job assignments',
    '<div class="technician-actions"><button class="button button-ghost" id="assignTechnicianJob">♧ Assign Technician</button></div>') +
    `<div class="technician-grid">${state.technicians.length ? state.technicians.map(tech => {
      const code = initials(tech.name);
      const currentJobs = tech.currentJobs || 0;
      const capacity = tech.capacity || 5;
      const workload = Math.min(100, Math.round((currentJobs / capacity) * 100));
      const availabilityClass = tech.availability === 'Available' ? 'status-green' : tech.availability === 'Busy' ? 'status-orange' : 'status-gray';
      return `<article class="technician-card">
        <div class="technician-card-top"><span class="technician-avatar">${escapeHtml(code)}</span><div><h3>${escapeHtml(tech.name)}</h3><p>${escapeHtml(tech.specialty || 'Repair technician')}</p></div></div>
        <div class="technician-statuses"><span class="status ${availabilityClass}">${tech.availability}</span><span class="status status-blue">Active</span></div>
        <div class="technician-metrics"><div><b>${currentJobs}</b><small>Current Jobs</small></div><div><b>${tech.completed}</b><small>Completed</small></div></div>
        <div class="technician-workload"><div><span>Workload</span><span>${currentJobs}/${capacity}</span></div><i><em style="width:${workload}%"></em></i></div>
        <div class="technician-card-actions"><button class="button button-ghost view-technician-profile" data-technician="${escapeHtml(tech.name)}" data-specialty="${escapeHtml(tech.specialty || 'Repair technician')}" data-availability="${escapeHtml(tech.availability)}" data-current-jobs="${currentJobs}" data-completed="${tech.completed}" data-capacity="${capacity}">View Profile</button><button class="button button-primary assign-technician-card" data-technician="${escapeHtml(tech.name)}">Assign Job</button></div>
      </article>`;
    }).join('') : `<section class="panel">${state.techniciansLoaded ? '<h3>No active technicians</h3><p>Active technician accounts will appear here.</p>' : '<h3>Loading technicians...</h3>'}</section>`}</div>`;
}

function renderInventory() {
  const parts = state.parts;
  const stockStatus = part => part.quantity === 0 ? '<span class="inventory-status inventory-out">Out of Stock</span>' : part.quantity <= part.reorderLevel ? '<span class="inventory-status inventory-low">Low Stock</span>' : '<span class="inventory-status inventory-in">In Stock</span>';
  const inventoryValue = parts.reduce((total, part) => total + part.quantity * part.unitCost, 0);
  const inStockCount = parts.filter(part => part.quantity > part.reorderLevel).length;
  const lowStockCount = parts.filter(part => part.quantity > 0 && part.quantity <= part.reorderLevel).length;
  const outOfStockCount = parts.filter(part => part.quantity === 0).length;
  const rows = parts.map(part => `<tr><td><span class="inventory-id">P-${String(part.partId).padStart(4, '0')}</span></td><td><b>${escapeHtml(part.name)}</b></td><td><span class="inventory-category">${escapeHtml(part.category || '—')}</span></td><td>${escapeHtml(part.brand || '—')}</td><td>${escapeHtml(part.description || '—')}</td><td><b class="${part.quantity === 0 ? 'qty-out' : part.quantity <= part.reorderLevel ? 'qty-low' : 'qty-good'}">${part.quantity}</b></td><td>${part.reorderLevel}</td><td><b>${money(part.unitCost)}</b></td><td>${escapeHtml(part.supplier || '—')}</td><td>${stockStatus(part)}</td><td class="record-actions"><button type="button" class="inventory-icon-button edit-part" data-part-id="${part.partId}" title="Edit part" aria-label="Edit part">✎</button></td></tr>`).join('');
  const canManage = ['ADMIN', 'INVENTORY', 'MANAGER'].includes(state.currentUserRole);
  const actions = canManage ? '<div class="inventory-actions"><button type="button" class="button inventory-stock-in">⇩ Stock In</button><button type="button" class="button inventory-stock-out">⇧ Stock Out</button><button type="button" class="button button-primary">＋ Add Part</button></div>' : '';
  return layout('Parts Inventory', 'Manage parts and stock levels',
    actions) +
    `<div class="inventory-summary"><div><b>${parts.length}</b><span>Total Parts</span></div><div><b class="summary-green">${inStockCount}</b><span>In Stock</span></div><div><b class="summary-orange">${lowStockCount}</b><span>Low Stock</span></div><div><b class="summary-red">${outOfStockCount}</b><span>Out of Stock</span></div><div><b class="summary-teal">${money(inventoryValue)}</b><span>Inventory Value</span></div></div>
    <section class="panel inventory-panel"><input class="table-search inventory-search" id="inventorySearch" placeholder="⌕  Search by name, ID, or category..."><div class="table-wrap"><table class="data-table inventory-table"><thead><tr><th>Part ID</th><th>Name</th><th>Category</th><th>Brand</th><th>Description</th><th>Qty</th><th>Reorder</th><th>Unit cost</th><th>Supplier</th><th>Status</th><th>Actions</th></tr></thead><tbody>${rows || `<tr><td colspan="11">${state.partsLoaded ? 'No inventory parts recorded.' : 'Loading inventory...'}</td></tr>`}</tbody></table></div></section>`;
}

function renderSuppliers() {
  const supplierRows = state.suppliers.map(supplier => `<tr data-supplier-id="${supplier.supplierId}">
    <td>${person(escapeHtml(supplier.name), initials(supplier.name))}</td>
    <td><span class="tag">${escapeHtml(supplier.category || '')}</span></td>
    <td>${escapeHtml(supplier.contactPerson || '')}</td>
    <td>${escapeHtml(supplier.contactEmail || '')}</td>
    <td>${status(supplier.status)}</td>
    <td><button class="button button-ghost supplier-view" data-supplier-id="${supplier.supplierId}">View</button></td>
  </tr>`).join('');

  return layout('Suppliers', 'Manage hardware vendors and keep purchasing contacts organized.', '<button class="button button-primary" id="newSupplier">＋ Add supplier</button>') +
    `<div class="metric-grid supplier-summary"><div class="panel metric"><label>Total suppliers</label><strong>${state.suppliers.length}</strong><small>Registered vendors</small></div><div class="panel metric"><label>Active suppliers</label><strong>${state.suppliers.filter(s => s.status === 'Active').length}</strong><small>Available for purchasing</small></div><div class="panel metric"><label>Categories</label><strong>${new Set(state.suppliers.map(s => s.category).filter(Boolean)).size}</strong><small>Parts coverage</small></div></div>
    <section class="panel supplier-dashboard-panel"><div class="toolbar"><input class="table-search" id="supplierDashboardSearch" placeholder="⌕  Search supplier, contact, or category..."><select class="select" id="supplierDashboardStatus"><option>All statuses</option><option>Active</option><option>Inactive</option></select><span class="spacer"></span><button class="button button-ghost" id="exportSupplierDashboard">⇩ Export CSV</button></div><div class="table-wrap"><table class="data-table" id="supplierDashboardTable"><thead><tr><th>Supplier</th><th>Category</th><th>Contact</th><th>Email</th><th>Status</th><th>Action</th></tr></thead><tbody>${supplierRows || `<tr><td colspan="6">${state.suppliersLoaded ? 'No suppliers registered.' : 'Loading suppliers...'}</td></tr>`}</tbody></table></div></section>`;
}

function renderBillingLiveMarkup() {
  const invoices = state.invoices || [];
  const collected = invoices.reduce((total, invoice) => total + parseMoney(invoice.paid), 0);
  const outstanding = invoices.reduce((total, invoice) => total + Math.max(0, parseMoney(invoice.amount) - parseMoney(invoice.paid)), 0);
  const average = invoices.length ? invoices.reduce((total, invoice) => total + parseMoney(invoice.amount), 0) / invoices.length : 0;
  const unpaidCount = invoices.filter(invoice => invoice.status !== 'Paid').length;
  const rows = invoices.length ? invoices.map(invoice => {
    const amount = parseMoney(invoice.amount);
    const paid = parseMoney(invoice.paid);
    const balance = Math.max(0, amount - paid);
    const invoiceId = invoice.invoiceId || invoice.id;
    return `<tr><td><b>${invoice.id}</b></td><td>${invoice.job}</td><td>${person(invoice.customer)}</td><td><b>${money(amount)}</b></td><td>${money(paid)}</td><td><b>${money(balance)}</b></td><td>${status(invoice.status)}</td><td>${invoice.date}</td><td>${balance > 0 ? `<button class="button button-primary record-payment" data-invoice-id="${invoiceId}">Record payment</button>` : `<button class="button button-ghost view-receipt" data-invoice-id="${invoiceId}">Receipt</button>`}</td></tr>`;
  }).join('') : '<tr><td colspan="9">No invoices found.</td></tr>';
  return layout('Billing & invoices','Create invoices, record payments and keep balances clear.','<button class="button button-primary">＋ Create invoice</button>')+
    `<div class="metric-grid"><div class="panel metric"><label>Collected</label><strong>${money(collected)}</strong><small>Recorded payments</small></div><div class="panel metric"><label>Outstanding balance</label><strong style="color:var(--orange)">${money(outstanding)}</strong><small style="color:var(--orange)">${unpaidCount} invoices unpaid</small></div><div class="panel metric"><label>Average invoice</label><strong>${money(average)}</strong><small>Across ${invoices.length} invoices</small></div></div><section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search invoice or customer..." aria-label="Search invoice or customer"><select class="select" aria-label="Filter payment status"><option>All payment statuses</option><option>Paid</option><option>Partially Paid</option><option>Unpaid</option></select><span class="spacer"></span><span class="muted">Live database data</span></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Invoice</th><th>Job order</th><th>Customer</th><th>Amount</th><th>Paid</th><th>Balance</th><th>Status</th><th>Date</th><th>Action</th></tr></thead><tbody>${rows}</tbody></table></div></section>`;
}

function renderBilling() {
  return renderBillingLiveMarkup();
  /* Existing static billing markup remains below as a fallback reference. */
    return layout('Billing & invoices','Create invoices, record payments and keep balances clear.','<button class="button button-primary">＋ Create invoice</button>')+
      `<div class="metric-grid"><div class="panel metric"><label>Collected</label><strong>${money(collected)}</strong><small>Recorded payments</small></div><div class="panel metric"><label>Outstanding balance</label><strong style="color:var(--orange)">${money(outstanding)}</strong><small style="color:var(--orange)">${unpaidCount} invoices unpaid</small></div><div class="panel metric"><label>Average invoice</label><strong>${money(average)}</strong><small>Across ${invoices.length} invoices</small></div></div><section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search invoice or customer..." aria-label="Search invoice or customer"><select class="select" aria-label="Filter payment status"><option>All payment statuses</option><option>Paid</option><option>Partially Paid</option><option>Unpaid</option></select><span class="spacer"></span><span class="muted">Live database data</span></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Invoice</th><th>Job order</th><th>Customer</th><th>Amount</th><th>Paid</th><th>Balance</th><th>Status</th><th>Date</th><th>Action</th></tr></thead><tbody>${rows}</tbody></table></div></section>`;
}

function showReceipt(invoice, paymentAmount, paymentMethod) {
  const subtotal = paymentAmount / 1.12;
  const tax = paymentAmount - subtotal;
  const receiptNumber = `RCT-${Date.now().toString().slice(-8)}`;
  const backdrop = document.createElement('div');
  backdrop.className = 'receipt-backdrop';
  backdrop.innerHTML = `<section class="receipt" role="dialog" aria-modal="true" aria-labelledby="receiptTitle"><div class="receipt-header"><div><p class="eyebrow">TechServe payment</p><h2 id="receiptTitle">Payment receipt</h2><p>${receiptNumber}</p></div><button class="modal-close receipt-close" aria-label="Close receipt">×</button></div><div class="receipt-details"><div><span>Invoice</span><b>${invoice.id}</b></div><div><span>Customer</span><b>${invoice.customer}</b></div><div><span>Payment method</span><b>${paymentMethod}</b></div><div><span>Date</span><b>${new Date().toLocaleString()}</b></div></div><div class="receipt-totals"><p><span>Subtotal</span><b>${money(subtotal)}</b></p><p><span>Tax (12%)</span><b>${money(tax)}</b></p><p class="receipt-total"><span>Total paid</span><b>${money(paymentAmount)}</b></p></div><button class="button button-primary print-receipt">▣ Print receipt</button></section>`;
  document.body.appendChild(backdrop);
  backdrop.querySelector('.receipt-close').onclick = () => backdrop.remove();
  backdrop.querySelector('.print-receipt').onclick = () => window.print();
}

function openPaymentModal(invoice) {
  const resolvedInvoiceId = resolveInvoiceId(invoice);
  if (!resolvedInvoiceId) {
    alert('Invoice was not found.');
    return;
  }

  const outstandingBalance = parseMoney(invoice.amount) - parseMoney(invoice.paid);
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop open';
  backdrop.id = 'paymentModalBackdrop';
  backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="paymentModalTitle" aria-describedby="paymentBalance paymentValidation"><div class="modal-header"><div><p class="eyebrow">Billing & invoices</p><h2 id="paymentModalTitle">Enter Payment</h2></div><button type="button" class="modal-close" id="paymentModalClose" aria-label="Close payment dialog">×</button></div><form id="paymentForm"><div class="form-grid"><p class="wide" id="paymentBalance" style="margin:0;color:var(--muted);font-size:13px">Outstanding Balance: <strong style="color:var(--ink)">${money(outstandingBalance)}</strong></p><label class="wide">Payment Amount<input id="paymentAmount" type="number" min="0.01" max="${outstandingBalance.toFixed(2)}" step="0.01" value="${outstandingBalance.toFixed(2)}" inputmode="decimal" required aria-describedby="paymentValidation"></label><label class="wide">Payment Method<select id="paymentMethod"><option>Cash</option><option>GCash</option><option>Bank Transfer</option><option>Card</option></select></label><p class="login-error wide" id="paymentValidation" role="alert" style="display:none;margin:0"></p></div><div class="modal-actions"><button type="button" class="button button-ghost" id="paymentCancel">Cancel</button><button type="submit" class="button button-primary">Confirm Payment</button></div></form></section>`;
  document.body.appendChild(backdrop);

  const form = backdrop.querySelector('#paymentForm');
  const amountInput = backdrop.querySelector('#paymentAmount');
  const validation = backdrop.querySelector('#paymentValidation');
  const close = () => {
    document.removeEventListener('keydown', handleKeydown);
    backdrop.remove();
  };
  const handleKeydown = event => {
    if (event.key === 'Escape') close();
  };
  const showValidation = message => {
    validation.textContent = message;
    validation.style.display = 'block';
    amountInput.setAttribute('aria-invalid', 'true');
  };

  backdrop.querySelector('#paymentModalClose').onclick = close;
  backdrop.querySelector('#paymentCancel').onclick = close;
  backdrop.onclick = event => { if (event.target === backdrop) close(); };
  document.addEventListener('keydown', handleKeydown);
  form.onsubmit = async event => {
    event.preventDefault();
    const paymentAmount = Number(amountInput.value);
    if (!Number.isFinite(paymentAmount) || paymentAmount <= 0) {
      showValidation('Enter a payment amount greater than ₱0.00.');
      amountInput.focus();
      return;
    }
    if (paymentAmount > outstandingBalance) {
      showValidation(`Payment cannot exceed the outstanding balance of ${money(outstandingBalance)}.`);
      amountInput.focus();
      return;
    }

    const paymentMethod = backdrop.querySelector('#paymentMethod').value;
    const confirmButton = form.querySelector('button[type="submit"]');
    confirmButton.disabled = true;
    confirmButton.textContent = 'Processing...';
    try {
      const response = await fetch(`/api/billing/${resolvedInvoiceId}/payments`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ amount: paymentAmount, paymentMethod })
      });
      const result = await response.json().catch(() => ({}));
      if (!response.ok || !result.invoice) {
        showValidation(result.error || 'Unable to record payment. Please try again.');
        confirmButton.disabled = false;
        confirmButton.textContent = 'Confirm Payment';
        return;
      }

      const updatedInvoice = normalizeBillingInvoice(result.invoice);
      state.invoices = state.invoices.map(item => resolveInvoiceId(item) === updatedInvoice.invoiceId ? updatedInvoice : item);
      state.billingLoaded = true;
      close();
      render();
      showReceipt(updatedInvoice, Number(result.paymentAmount), result.paymentMethod);
    } catch (err) {
      showValidation('Unable to connect to billing. Please try again.');
      confirmButton.disabled = false;
      confirmButton.textContent = 'Confirm Payment';
    }
  };
  window.setTimeout(() => amountInput.focus(), 0);
}

function completePayment(invoice) {
  openPaymentModal(invoice);
}

function renderCrm() {
  const liveCustomers = (state.customers || []).map((customer, index) => {
    const nameParts = String(customer.name || 'Customer').split(' ');
    const initialsText = nameParts.slice(0, 2).map(part => part[0] || '').join('').toUpperCase() || 'CU';
    const customerId = customer.customerId || customer.id || `CUS-${String(index + 1).padStart(4, '0')}`;
    return {
      name: customer.name || 'Customer',
      id: customer.id || customerId,
      initials: initialsText,
      phone: customer.contact || '—',
      email: customer.email || '—',
      address: customer.address || '—',
      device: 'Latest device',
      job: customer.jobs ? `Repair jobs: ${customer.jobs}` : 'No jobs',
      status: customer.status || 'Active',
      cost: customer.jobs ? 'Live data' : '—',
      invoice: '—',
      date: '—',
      paid: customer.status || 'Active',
      repairs: Number(customer.jobs) || 0,
      spent: customer.jobs ? 'Live data' : '—',
      joined: 'Live DB',
      notes: 0
    };
  });
  const fallbackCustomers = [
    { name: 'Maria Santos', id: 'C-001', initials: 'MS', phone: '0917-234-5678', email: 'maria.santos@gmail.com', address: '42 Rizal St, Makati', device: 'Lenovo ThinkPad E15', job: 'RJ-2024-001', status: 'In Repair', cost: '₱3,500', invoice: 'INV-2024-003', date: '2024-11-22', paid: 'Partially Paid', repairs: 7, spent: '₱3,500', joined: 'Nov 18', notes: 3 },
    { name: 'Jose Reyes', id: 'C-002', initials: 'JR', phone: '0918-456-7890', email: 'jose.reyes@gmail.com', address: '18 Mabini St, Quezon City', device: 'HP Pavilion TP01', job: 'RJ-2024-002', status: 'Ready for Pickup', cost: '₱1,800', invoice: 'INV-2024-004', date: '2024-11-21', paid: 'Paid', repairs: 4, spent: '₱8,200', joined: 'Nov 17', notes: 2 },
    { name: 'Ana Villanueva', id: 'C-003', initials: 'AV', phone: '0919-222-3456', email: 'ana.villanueva@gmail.com', address: '9 Jupiter St, Makati', device: 'MacBook Air M2', job: 'RJ-2024-003', status: 'Waiting for Parts', cost: '₱6,500', invoice: 'INV-2024-005', date: '2024-11-20', paid: 'Partially Paid', repairs: 3, spent: '₱12,500', joined: 'Nov 15', notes: 4 },
    { name: 'Marco Dela Rosa', id: 'C-004', initials: 'MD', phone: '0920-333-4567', email: 'marco.delarosa@gmail.com', address: '7 P. Gomez St, Manila', device: 'Dell XPS 15', job: 'RJ-2024-006', status: 'Completed', cost: '₱8,500', invoice: 'INV-2024-006', date: '2024-11-19', paid: 'Paid', repairs: 6, spent: '₱24,000', joined: 'Nov 16', notes: 5 }
  ];
  const customers = liveCustomers.length ? liveCustomers : fallbackCustomers;
  const customer = customers[state.crmCustomerIndex] || customers[0];
  return `<div class="crm-heading"><div><h1>Customer CRM</h1><p>Customer relationships and service history</p></div><div class="crm-customer-tabs">${customers.map((item, index) => `<button class="${index === state.crmCustomerIndex ? 'active' : ''}" data-crm-customer="${index}">${item.name.split(' ')[0]}</button>`).join('')}</div></div>
    <div class="crm-layout"><aside class="crm-profile-card"><div class="crm-avatar">${customer.initials}</div><h2>${customer.name}</h2><p class="crm-id">${customer.id}</p><span class="crm-active">● Active Customer</span><div class="crm-contact"><p>♧ ${customer.phone}</p><p>✉ ${customer.email}</p><p>⌖ ${customer.address}</p></div><div class="crm-profile-actions"><button class="button button-ghost">Edit</button></div><div class="crm-stats"><div><b>${customer.repairs}</b><small>Total Repairs</small></div><div><b>${customer.spent}</b><small>Total Spent</small></div><div><b>${customer.joined}</b><small>Last Visit</small></div><div><b>${customer.notes}</b><small>Notes</small></div></div></aside>
    <main class="crm-details"><section class="panel crm-section"><h3>Repair History</h3><table class="data-table"><thead><tr><th>JOB ID</th><th>DEVICE</th><th>PROBLEM</th><th>STATUS</th><th>COST</th></tr></thead><tbody><tr><td><b class="repair-id">${customer.job}</b></td><td>${customer.device}</td><td>Laptop not turning on, battery drains immediately</td><td>${status(customer.status)}</td><td><b>${customer.cost}</b></td></tr></tbody></table></section>
    <section class="panel crm-section"><h3>Billing History</h3><table class="data-table"><thead><tr><th>INVOICE</th><th>DATE</th><th>TOTAL</th><th>STATUS</th></tr></thead><tbody><tr><td><b class="repair-id">${customer.invoice}</b></td><td>${customer.date}</td><td><b>${customer.cost}</b></td><td>${status(customer.paid)}</td></tr></tbody></table></section>
    <section class="panel crm-section crm-timeline"><h3>Customer Timeline</h3><div><p>● &nbsp; New repair job created: ${customer.job}<small>Nov 18, 2024</small></p><p>● &nbsp; Payment received: ${customer.cost} for RJ-2024-006<small>Nov 10, 2024</small></p><p>● &nbsp; Device checked in: ${customer.device}<small>Oct 28, 2024</small></p><p>● &nbsp; Customer note updated by Mark Bautista</p></div></section></main></div>`;
}

function renderReportsLiveMarkup() {
  const summary = state.reportSummary || {
    revenue: 0,
    completedRepairs: 0,
    technicians: [],
    deviceTypes: []
  };
  const technicians = summary.technicians || [];
  const deviceTypes = summary.deviceTypes || [];
  const deviceTotal = deviceTypes.reduce((total, item) => total + item.repairs, 0);
  const formatReportMoney = value => `₱${Number(value || 0).toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  const technicianRows = technicians.length ? technicians.map(item => `<tr><td>${person(item.name)}</td><td><b>${item.completed}</b></td><td>${Number(item.averageTurnaroundDays || 0).toFixed(1)} days</td><td>Live database data</td></tr>`).join('') : '<tr><td colspan="4">No completed repair data yet.</td></tr>';
  const deviceRows = deviceTypes.length ? deviceTypes.map(item => `<div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>${item.name || 'Other'}</b><small>${item.repairs} repairs</small></div><b>${Number(item.percentage || 0).toFixed(0)}%</b></div>`).join('') : '<div class="alert"><div style="flex:1"><b>No repair data yet</b></div></div>';
  return layout('Reports & analytics','Understand revenue, repair throughput and technician performance.','<button class="button button-ghost">▣ Print report</button>')+
    `<div class="toolbar"><span class="muted">Live database metrics</span><span class="spacer"></span><button class="button button-light">⇩ Export CSV</button></div><div class="dashboard-grid"><section class="panel"><div class="panel-header"><div><h3>Technician performance</h3><p>Completed repairs and average turnaround</p></div></div><table class="data-table"><thead><tr><th>Technician</th><th>Completed</th><th>Avg. time</th><th>Source</th></tr></thead><tbody>${technicianRows}</tbody></table></section><section class="panel"><div class="panel-header"><div><h3>Most repaired device types</h3><p>${deviceTotal} completed repairs</p></div></div><div class="alert-list">${deviceRows}</div></section></div><section class="panel recent"><div class="panel-header"><div><h3>Report snapshot</h3><p>Updated automatically from the current database</p></div></div><div class="metric-grid"><div class="metric"><label>Revenue</label><strong>${formatReportMoney(summary.revenue)}</strong><small>Invoice total</small></div><div class="metric"><label>Repairs completed</label><strong>${summary.completedRepairs}</strong><small>Completed repair jobs</small></div><div class="metric"><label>Device types</label><strong>${deviceTypes.length}</strong><small>Recorded in completed jobs</small></div></div></section>`;
}

function renderReports() {
  return renderReportsLiveMarkup();
  /* Existing static report markup remains below as a fallback reference. */
  return layout('Reports & analytics','Understand revenue, repair throughput and technician performance.','<button class="button button-ghost">▣ Print report</button>')+
    `<div class="toolbar"><select class="select"><option>September 2024</option><option>August 2024</option><option>July 2024</option></select><select class="select"><option>All locations</option><option>TechServe HQ</option></select><span class="spacer"></span><button class="button button-light">⇩ Export CSV</button></div><div class="dashboard-grid"><section class="panel"><div class="panel-header"><div><h3>Technician performance</h3><p>Completed repairs and average turnaround</p></div></div><table class="data-table"><thead><tr><th>Technician</th><th>Completed</th><th>Avg. time</th><th>Customer rating</th></tr></thead><tbody><tr><td>${person('Noah Williams')}</td><td><b>42</b></td><td>2.4 days</td><td><span style="color:#e99a36">★★★★★</span> 4.9</td></tr><tr><td>${person('Sofia Patel')}</td><td><b>38</b></td><td>2.1 days</td><td><span style="color:#e99a36">★★★★★</span> 4.8</td></tr><tr><td>${person('Liam Chen')}</td><td><b>31</b></td><td>2.8 days</td><td><span style="color:#e99a36">★★★★☆</span> 4.6</td></tr></tbody></table></section><section class="panel"><div class="panel-header"><div><h3>Most repaired device types</h3><p>September repair volume</p></div></div><div class="alert-list"><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>Laptops</b><small>82 repairs</small></div><b>46%</b></div><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>MacBooks</b><small>44 repairs</small></div><b>25%</b></div><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>Desktops</b><small>31 repairs</small></div><b>17%</b></div></div></section></div><section class="panel recent"><div class="panel-header"><div><h3>Report snapshot</h3><p>Key operational metrics for this period</p></div></div><div class="metric-grid"><div class="metric"><label>Revenue</label><strong>$24,680</strong><small>+18.6% month over month</small></div><div class="metric"><label>Repairs completed</label><strong>126</strong><small>+15.2% month over month</small></div><div class="metric"><label>Avg. ticket value</label><strong>$195.87</strong><small>+4.8% month over month</small></div></div></section>`;
}

function renderProfile() {
  const userName = window.__TECHSERVE_USER__?.name || 'User';
  const role = state.currentUserRole || 'ADMIN';
  const colorClass = roleColorClass(role);
  const email = `${userName.toLowerCase().replace(/\s+/g, '.')}@techserve.example`;
  const picture = window.__TECHSERVE_USER__?.picture || '';
  return layout('Profile','Manage your administrator account and workspace access.','<button class="button button-primary" data-change-password>Change password</button>')+
    `<section class="panel"><div class="panel-header"><div><h3>Administrator profile</h3><p>Primary account information for your TechServe workspace.</p></div><button type="button" class="button button-primary" data-save-profile>Save changes</button></div><div class="profile-layout" style="display:grid;grid-template-columns:240px 1fr;gap:22px;align-items:start;padding:12px 0 4px;">
      <div style="background:#f5f8ff;border:1px solid #e4edf8;border-radius:18px;padding:24px 18px;text-align:center;">
        <div id="profileAvatar" style="width:96px;height:96px;border-radius:50%;margin:0 auto 14px;background:linear-gradient(135deg,#3f67eb,#7d5ae9);display:grid;place-items:center;overflow:hidden;color:#fff;font-size:28px;font-weight:800;box-shadow:0 18px 30px rgba(63,103,235,.22);">${picture ? `<img src="${escapeHtml(picture)}" alt="Profile picture" style="width:100%;height:100%;object-fit:cover;">` : initials(userName).toUpperCase()}</div>
        <h3 style="margin:0 0 6px;font-size:18px;">${userName}</h3>
        <p class="role-label ${colorClass}">${roleLabel(role)}</p>
      </div>
      <div class="form-grid" style="max-width:700px;">
        <label>Admin name<input id="profileNameInput" value="${escapeHtml(userName)}" maxlength="120" required></label>
        <label>Email<input value="${escapeHtml(email)}"></label>
        <label>Role<select>
          <option selected>${roleLabel(role)}</option>
          <option>Administrator</option>
          <option>Manager</option>
          <option>Technician</option>
          <option>Staff</option>
        </select></label>
        <label>Profile picture<input id="profilePictureInput" type="file" accept="image/png,image/jpeg,image/webp"></label>
        <div style="display:flex;align-items:center;gap:10px;grid-column:1/-1; margin-top:8px;">
          <button type="button" class="button button-primary" data-change-password>Change password</button>
          <button type="button" class="button button-ghost" data-menu-action="settings">Open settings</button>
        </div>
      </div>
    </div></section>`;
}

function openPasswordChangeModal() {
  const form = document.getElementById('passwordChangeForm');
  if (!form) return;
  form.reset();
  const message = document.getElementById('passwordChangeMessage');
  if (message) {
    message.textContent = '';
    message.className = 'form-message';
  }
  document.getElementById('passwordChangeModalBackdrop')?.classList.add('open');
}

function closePasswordChangeModal() {
  document.getElementById('passwordChangeModalBackdrop')?.classList.remove('open');
  const form = document.getElementById('passwordChangeForm');
  if (form) form.reset();
}

function renderSettings() {
  return layout('Settings','Manage your workspace preferences and staff access.','<button class="button button-primary">Save changes</button>')+
    `<section class="panel"><div class="panel-header"><div><h3>Workspace settings</h3><p>These settings apply to your TechServe workspace.</p></div></div><div class="form-grid" style="max-width:700px"><label>Business name<input value="TechServe HQ"></label><label>Business email<input value="hello@techserve.example"></label><label>Timezone<select><option>Pacific Time (UTC-08:00)</option></select></label><label>Currency<select><option>PHP — Philippine Peso</option></select></label><label>Team members<input value="4 active members"></label><label>Appearance<select id="appearanceSelect"><option value="light">Light</option><option value="dark" ${localStorage.getItem('techserve_appearance') === 'dark' ? 'selected' : ''}>Dark</option></select></label><label>Other system settings<input value="2FA enabled, backups daily, audit logs on"></label></div></section><section class="panel recent"><div class="panel-header"><div><h3>Team members</h3><p>Role-based access for your repair shop</p></div><button class="button button-light">＋ Invite member</button></div><table class="data-table"><thead><tr><th>Member</th><th>Role</th><th>Last active</th><th>Status</th></tr></thead><tbody><tr><td>${person('Krestal','KR')}</td><td><span class="role-label role-orange">${roleLabel('MANAGER')}</span></td><td>Just now</td><td>${status('Completed')}</td></tr><tr><td>${person('Shielo','SH')}</td><td><span class="role-label role-green">${roleLabel('STAFF')}</span></td><td>Today, 9:18 AM</td><td>${status('Completed')}</td></tr><tr><td>${person('Czymon','CZ')}</td><td><span class="role-label role-blue">${roleLabel('TECHNICIAN')}</span></td><td>Today, 8:54 AM</td><td>${status('Completed')}</td></tr><tr><td>${person('Kim Mingyu','KM')}</td><td><span class="role-label role-blue">${roleLabel('TECHNICIAN')}</span></td><td>Today, 8:30 AM</td><td>${status('Completed')}</td></tr></tbody></table></section>`;
}

function setupProfileMenu() {
  const profile = document.querySelector('.profile');
  const trigger = profile?.querySelector('.more');
  const menu = document.getElementById('profileMenu');
  if (!profile || !trigger || !menu) return;

  if (profile.dataset.menuBound === 'true') return;
  profile.dataset.menuBound = 'true';

  trigger.addEventListener('click', (event) => {
    event.stopPropagation();
    const shouldOpen = menu.hidden;
    document.querySelectorAll('.profile-menu').forEach(item => item.hidden = true);
    menu.hidden = !shouldOpen;
    trigger.setAttribute('aria-expanded', String(!menu.hidden));
  });

  document.addEventListener('click', (event) => {
    if (!profile.contains(event.target)) {
      menu.hidden = true;
      trigger.setAttribute('aria-expanded', 'false');
    }
  });

  menu.querySelectorAll('[data-menu-action]').forEach((item) => {
    item.addEventListener('click', () => {
      const action = item.dataset.menuAction;
      menu.hidden = true;
      trigger.setAttribute('aria-expanded', 'false');
      if (action === 'profile') {
        window.location.href = '/Admin/Dashboard?view=profile';
      }
      if (action === 'settings') {
        window.location.href = '/Admin/Dashboard?view=settings';
      }
    });
  });
}

function render() {
  if (document.getElementById('appShell')?.dataset.page === 'user-management') {
    setupProfileMenu();
    return;
  }
  applyRoleProfile();
  applyNavigationAccess();
  const views = {
    overview: renderOverview,
    repairs: renderRepairs,
    tracking: renderTracking,
    customers: renderCustomers,
    devices: renderDevices,
    technicians: renderTechnicians,
    inventory: renderInventory,
    suppliers: renderSuppliers,
    billing: renderBilling,
    crm: renderCrm,
    reports: renderReports,
    profile: renderProfile,
    settings: renderSettings
  };
  const contentEl = document.getElementById('content');
  if (!contentEl) return;
  const renderFn = views[state.view] || renderOverview;
  contentEl.innerHTML = renderFn().replace(/\$/g, '₱');
  setupProfileMenu();
  const pageCrumb = document.getElementById('pageCrumb');
  if (pageCrumb) pageCrumb.textContent = state.view.charAt(0).toUpperCase() + state.view.slice(1);
  document.querySelectorAll('.nav-item[data-view]').forEach(item => item.classList.toggle('active', item.dataset.view === state.view));
  document.querySelector('.nav-item[href="/Admin/Dashboard"]')?.classList.toggle('active', state.view === 'overview');
  bindViewActions();
  if ((state.view === 'overview' || state.view === 'tracking' || state.view === 'repairs') && !state.trackingLoaded) loadLiveRepairTracking();
  if (state.view === 'technicians' && !state.techniciansLoaded) loadLiveTechnicians();
  if (state.view === 'devices' && !state.devicesLoaded) loadLiveDevices();
  if (state.view === 'inventory' && !state.partsLoaded) loadLiveInventory();
  if (state.view === 'suppliers' && !state.suppliersLoaded) loadLiveSuppliers();
  if ((state.view === 'overview' || state.view === 'customers' || state.view === 'crm') && !state.customersLoaded) loadLiveCustomers();
  if (state.view === 'billing' && !state.billingLoaded) loadLiveBilling();
  if (state.view === 'reports' && !state.reportsLoaded) loadLiveReports();
  startLiveDashboardRefresh();
}

let liveDashboardRefreshTimer;

function startLiveDashboardRefresh() {
  if (liveDashboardRefreshTimer) return;
  liveDashboardRefreshTimer = window.setInterval(() => {
    if (['overview', 'tracking', 'repairs'].includes(state.view)) {
      state.trackingLoaded = false;
      loadLiveRepairTracking();
    }
    if (state.view === 'technicians') {
      state.techniciansLoaded = false;
      loadLiveTechnicians();
    }
    if (state.view === 'devices') {
      state.devicesLoaded = false;
      loadLiveDevices();
    }
    if (state.view === 'inventory') {
      state.partsLoaded = false;
      loadLiveInventory();
    }
    if (state.view === 'suppliers') {
      state.suppliersLoaded = false;
      loadLiveSuppliers();
    }
    if (['overview', 'customers', 'crm'].includes(state.view)) {
      state.customersLoaded = false;
      loadLiveCustomers();
    }
    if (state.view === 'billing') {
      state.billingLoaded = false;
      loadLiveBilling();
    }
    if (state.view === 'reports') {
      state.reportsLoaded = false;
      loadLiveReports();
    }
  }, 15000);
}

async function loadLiveTechnicians() {
  try {
    const response = await fetch('/api/technicians');
    if (!response.ok) throw new Error(`Technician request failed: ${response.status}`);
    state.technicians = await response.json();
    state.techniciansLoaded = true;
    if (state.view === 'technicians') render();
  } catch (error) {
    state.techniciansLoaded = true;
    console.warn('Unable to load technician data:', error);
    if (state.view === 'technicians') render();
  }
}

async function loadLiveDevices() {
  try {
    const response = await fetch('/api/devices');
    if (!response.ok) throw new Error(`Device request failed: ${response.status}`);
    const records = await response.json();
    state.devices = records.map(device => ({
      ...device,
      id: `DEV-${String(device.deviceId).padStart(4, '0')}`,
      device: [device.brand, device.model, device.deviceType].filter(Boolean).join(' '),
      serial: device.serialNumber || ''
    }));
    state.devicesLoaded = true;
    if (state.view === 'devices') render();
  } catch (error) {
    state.devicesLoaded = true;
    console.warn('Unable to load devices:', error);
    if (state.view === 'devices') render();
  }
}

  async function loadLiveInventory() {
    try {
      const response = await fetch('/api/inventory');
      if (!response.ok) throw new Error(`Inventory request failed: ${response.status}`);
      state.parts = await response.json();
      state.partsLoaded = true;
      if (state.view === 'inventory') render();
    } catch (error) {
      state.partsLoaded = true;
      console.warn('Unable to load inventory:', error);
      if (state.view === 'inventory') render();
    }
  }

  async function openInventoryPartDialog(part = null) {
    let suppliers;
    try {
      const response = await fetch('/api/suppliers');
      if (!response.ok) throw new Error(`Supplier request failed: ${response.status}`);
      suppliers = (await response.json()).filter(supplier => supplier.status === 'Active');
    } catch (error) {
      console.warn('Unable to load suppliers for inventory:', error);
      showActionMessage('Unable to load suppliers. Please try again.');
      return;
    }

    openActionDialog(part ? 'Edit part' : 'Add part', [
      { name: 'name', label: 'Part name', value: part?.name, required: true },
      { name: 'category', label: 'Category', value: part?.category },
      { name: 'brand', label: 'Brand', value: part?.brand },
      { name: 'description', label: 'Description', value: part?.description, wide: true },
      { name: 'reorderLevel', label: 'Reorder level', type: 'number', value: part?.reorderLevel ?? 5, required: true },
      { name: 'unitCost', label: 'Unit cost', type: 'number', value: part?.unitCost ?? 0, required: true },
      { name: 'sellingPrice', label: 'Selling price', type: 'number', value: part?.sellingPrice ?? 0, required: true },
      { name: 'supplierId', label: 'Supplier', type: 'select', options: [{ value: '', label: 'No supplier' }, ...suppliers.map(supplier => ({ value: supplier.supplierId, label: supplier.name }))], value: part?.supplierId ?? '' }
    ], async values => {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const request = {
        name: values.name,
        category: values.category,
        brand: values.brand,
        description: values.description,
        quantity: part?.quantity ?? 0,
        reorderLevel: Number(values.reorderLevel),
        unitCost: Number(values.unitCost),
        sellingPrice: Number(values.sellingPrice),
        supplierId: Number(values.supplierId) || null
      };
      try {
        const response = await fetch(part ? `/api/inventory/${part.partId}` : '/api/inventory', {
          method: part ? 'PUT' : 'POST',
          headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
          body: JSON.stringify(request)
        });
        if (!response.ok) {
          const error = await response.json().catch(() => ({ error: 'Part could not be saved.' }));
          showActionMessage(error.error || 'Part could not be saved.');
          return false;
        }
        state.view = 'inventory';
        state.partsLoaded = false;
        await loadLiveInventory();
        showActionMessage(part ? 'Part updated.' : 'Part added.');
        return true;
      } catch (error) {
        console.warn('Unable to save part:', error);
        showActionMessage('Unable to save the part. Please try again.');
        return false;
      }
    });
  }

  async function openStockAdjustmentDialog(direction) {
    if (!state.partsLoaded) await loadLiveInventory();
    if (direction === 'IN' && !state.suppliersLoaded) await loadLiveSuppliers();
    const options = state.parts.map(part => ({
      value: part.partId,
      label: `P-${String(part.partId).padStart(4, '0')} · ${part.name} (${part.quantity} in stock)`
    }));
    if (!options.length) {
      showActionMessage('Add a part before adjusting stock.');
      return;
    }
    const supplierOptions = direction === 'IN'
      ? state.suppliers.filter(supplier => supplier.status === 'Active').map(supplier => ({ value: supplier.supplierId, label: supplier.name }))
      : [];
    if (direction === 'IN' && !supplierOptions.length) {
      showActionMessage('Add an active supplier before recording stock-in.');
      return;
    }

    const fields = [
      { name: 'partId', label: 'Part', type: 'select', options, required: true },
      { name: 'quantity', label: 'Quantity', type: 'number', required: true },
      ...(direction === 'IN' ? [
        { name: 'supplierId', label: 'Supplier', type: 'select', options: supplierOptions, required: true },
        { name: 'referenceNumber', label: 'Purchase reference' }
      ] : [])
    ];
    openActionDialog(direction === 'IN' ? 'Stock in' : 'Stock out', fields, async values => {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      try {
        const response = await fetch(`/api/inventory/${Number(values.partId)}/stock`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
          body: JSON.stringify({ direction, quantity: Number(values.quantity), supplierId: Number(values.supplierId) || null, referenceNumber: values.referenceNumber })
        });
        if (!response.ok) {
          const error = await response.json().catch(() => ({ error: 'Stock adjustment failed.' }));
          showActionMessage(error.error || 'Stock adjustment failed.');
          return false;
        }
        state.view = 'inventory';
        state.partsLoaded = false;
        await loadLiveInventory();
        return true;
      } catch (error) {
        console.warn('Unable to adjust stock:', error);
        showActionMessage('Unable to adjust stock. Please try again.');
        return false;
      }
    });
  }

  async function loadLiveSuppliers() {
    try {
      const response = await fetch('/api/suppliers');
      if (!response.ok) throw new Error(`Supplier request failed: ${response.status}`);
      state.suppliers = await response.json();
      state.suppliersLoaded = true;
      if (state.view === 'suppliers') render();
    } catch (error) {
      state.suppliersLoaded = true;
      console.warn('Unable to load suppliers:', error);
      if (state.view === 'suppliers') render();
    }
  }

function normalizeTrackingJob(item) {
  const received = new Date(item.dateReceived);
  const expected = item.expectedCompletionDate ? new Date(item.expectedCompletionDate) : null;
  return {
    repairJobId: item.repairJobId,
    id: item.jobId,
    jobId: item.jobId,
    complaint: item.complaint,
    technicianId: item.technicianId,
    customer: item.customer,
    customerContact: item.customerContact,
    customerEmail: item.customerEmail,
    device: item.device,
    deviceBrand: item.deviceBrand,
    deviceModel: item.deviceModel,
    deviceSerialNumber: item.deviceSerialNumber,
    deviceOperatingSystem: item.deviceOperatingSystem,
    tech: item.technician || 'Unassigned',
    diagnosis: item.diagnosis || '',
    repairDescription: item.repairDescription || '',
    partsUsed: item.partsUsed || [],
    due: expected ? expected.toISOString().slice(0, 10) : 'Not scheduled',
    received: received.toISOString().slice(0, 10),
    amount: `₱${Number(item.amount || 0).toLocaleString('en-PH', { minimumFractionDigits: 2 })}`,
    status: item.status,
    priority: item.priority || 'Normal',
    history: item.history || []
  };
}

async function loadLiveRepairTracking() {
  try {
    const response = await fetch('/api/repair-tracking');
    if (!response.ok) throw new Error(`Tracking request failed: ${response.status}`);
    const records = await response.json();
    state.trackingJobs = records.map(normalizeTrackingJob);
    state.jobs = state.trackingJobs.map(job => ({
      id: job.id,
      repairJobId: job.repairJobId,
      customer: job.customer,
      device: job.device,
      issue: job.complaint,
      tech: job.tech,
      technicianId: job.technicianId,
      status: job.status,
      priority: job.priority,
      due: job.due,
      amount: Number(job.amount.replace(/[^0-9.]/g, '')) || 0,
      progress: Math.round((['Received', 'Diagnosis', 'In Repair', 'Testing', 'Ready for Pickup', 'Completed'].indexOf(job.status) / 5) * 100)
    }));
    state.trackingLoaded = true;
    if (['overview', 'tracking', 'repairs'].includes(state.view)) render();
  } catch (error) {
    state.trackingLoaded = true;
    console.warn('Unable to load repair tracking data:', error);
  }
}

async function openRepairTrackingDetails(job) {
  if (!job) return;
  const stages = ['Received', 'Diagnosis', 'In Repair', 'Testing', 'Ready for Pickup', 'Completed'];
  const canUpdate = ['ADMIN', 'MANAGER', 'TECHNICIAN', 'STAFF'].includes(state.currentUserRole);
  const isTechnician = state.currentUserRole === 'TECHNICIAN';
  let availableParts = [];
  if (isTechnician) {
    try {
      const partsResponse = await fetch('/api/repair-parts/available');
      if (partsResponse.ok) availableParts = await partsResponse.json();
    } catch (error) {
      console.warn('Unable to load available repair parts:', error);
    }
  }
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop open';
  const history = (job.history || []).map(item => `<p><b>${escapeHtml(item.status)}</b> - ${new Date(item.changedAt).toLocaleString()} <small>by ${escapeHtml(item.changedBy || 'System')}</small></p>`).join('') || '<p>No status history recorded.</p>';
  const deviceDetails = [job.deviceBrand, job.deviceModel, job.device].filter(Boolean).map(escapeHtml).join(' · ');
  const customerDetails = [job.customerContact, job.customerEmail].filter(Boolean).map(escapeHtml).join(' · ');
  const partsList = job.partsUsed?.length
    ? `<ul>${job.partsUsed.map(part => `<li>${escapeHtml(part.partName)} × ${part.quantity}</li>`).join('')}</ul>`
    : '<p>No parts recorded for this job.</p>';
  const partOptions = availableParts.map(part => `<option value="${part.partId}" data-available="${part.quantityAvailable}">${escapeHtml(part.partName)} (${part.quantityAvailable} available)</option>`).join('');
  backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="trackingDetailsTitle"><div class="modal-header"><div><p class="eyebrow">Repair details</p><h2 id="trackingDetailsTitle">${escapeHtml(job.id)}</h2></div><button type="button" class="modal-close tracking-details-close" aria-label="Close repair details">×</button></div><div class="tracking-detail-content"><p><b>${escapeHtml(job.customer)}</b>${customerDetails ? ` · ${customerDetails}` : ''}</p><p>${deviceDetails}</p><p>${escapeHtml(job.tech)} · Expected: ${escapeHtml(job.due)}${job.deviceSerialNumber ? ` · Serial: ${escapeHtml(job.deviceSerialNumber)}` : ''}${job.deviceOperatingSystem ? ` · ${escapeHtml(job.deviceOperatingSystem)}` : ''}</p><div class="panel"><h3>Status History</h3><div class="tracking-history">${history}</div></div>${canUpdate ? `<form id="trackingStatusForm"><label>Update Status<select id="trackingStatusSelect">${stages.map(stage => `<option ${stage === job.status ? 'selected' : ''}>${stage}</option>`).join('')}</select></label><label>Diagnosis<textarea id="trackingDiagnosis" maxlength="1000" placeholder="Enter diagnosis">${escapeHtml(job.diagnosis)}</textarea></label><label>Repair progress<textarea id="trackingRepairDescription" maxlength="1000" placeholder="Record repair work completed">${escapeHtml(job.repairDescription)}</textarea></label><label>Repair note<textarea id="trackingStatusNote" maxlength="500" placeholder="Add a repair note"></textarea></label>${isTechnician ? `<div class="tracking-parts-used"><h3>Parts used</h3>${partsList}<label>Record part used<select id="trackingPartSelect"><option value="">Select available part</option>${partOptions}</select></label><label>Quantity<input id="trackingPartQuantity" type="number" min="1" value="1" ${availableParts.length ? '' : 'disabled'} /></label>${availableParts.length ? '' : '<p>No parts are currently in stock.</p>'}</div>` : ''}<div class="modal-actions"><button type="button" class="button button-ghost tracking-details-close">Cancel</button><button type="submit" class="button button-primary">Save repair update</button></div></form>` : '<p>Status updates are restricted to authorized staff.</p>'}</div></section>`;
  if (!isTechnician) {
    backdrop.querySelector('#trackingDiagnosis')?.closest('label')?.remove();
    backdrop.querySelector('#trackingRepairDescription')?.closest('label')?.remove();
  }
  document.body.appendChild(backdrop);
  const close = () => backdrop.remove();
  backdrop.querySelectorAll('.tracking-details-close').forEach(button => button.onclick = close);
  backdrop.onclick = event => { if (event.target === backdrop) close(); };
  const form = backdrop.querySelector('#trackingStatusForm');
  if (form) form.onsubmit = async event => {
    event.preventDefault();
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const selectedPartId = Number(document.getElementById('trackingPartSelect')?.value) || null;
    const response = await fetch(`/api/repair-tracking/${job.repairJobId}/status`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
      body: JSON.stringify({
        status: document.getElementById('trackingStatusSelect').value,
        note: document.getElementById('trackingStatusNote').value,
        diagnosis: document.getElementById('trackingDiagnosis')?.value ?? null,
        repairDescription: document.getElementById('trackingRepairDescription')?.value ?? null,
        partId: selectedPartId,
        partQuantity: selectedPartId ? Number(document.getElementById('trackingPartQuantity')?.value) || null : null
      })
    });
    if (!response.ok) {
      const error = await response.json().catch(() => ({ error: 'Unable to update repair status.' }));
      showActionMessage(error.error || 'Unable to update repair status.');
      return;
    }
    const updated = normalizeTrackingJob(await response.json());
    state.trackingJobs = state.trackingJobs.map(item => item.repairJobId === updated.repairJobId ? updated : item);
    const matchingJob = state.jobs.find(item => item.repairJobId === updated.repairJobId);
    if (matchingJob) Object.assign(matchingJob, {
      status: updated.status,
      diagnosis: updated.diagnosis,
      repairDescription: updated.repairDescription,
      partsUsed: updated.partsUsed
    });
    close();
    render();
  };
}

async function loadLiveCustomers() {
  try {
    const response = await fetch('/api/customers');
    if (!response.ok) return;
    const records = await response.json();
    state.customers = records.map(customer => ({
      customerId: customer.id,
      id: `CUS-${String(customer.id).padStart(4, '0')}`,
      name: customer.name,
      contact: customer.contact,
      email: customer.email,
      address: customer.address,
      jobs: customer.jobs,
      status: customer.status
    }));
    state.customersLoaded = true;
    if (['customers', 'overview'].includes(state.view)) {
      const contentEl = document.getElementById('content');
      if (contentEl) contentEl.innerHTML = renderCustomers();
      bindViewActions();
    }
  } catch (err) {
    state.customersLoaded = true;
    console.warn('Unable to load customers from API, using local fallback:', err);
  }
}

function normalizeBillingInvoice(item) {
  const amount = Number(item.amount || 0);
  const paid = Number(item.paid || 0);
  return {
    invoiceId: item.invoiceId,
    id: `INV-${String(item.invoiceId).padStart(4, '0')}`,
    job: `JOB-${String(item.repairJobId).padStart(4, '0')}`,
    customer: item.customer,
    amount: money(amount),
    paid: money(paid),
    status: item.status,
    date: new Date(item.invoiceDate).toLocaleDateString('en-PH', { month: 'short', day: '2-digit', year: 'numeric' })
  };
}

async function loadLiveBilling() {
  try {
    const response = await fetch('/api/billing');
    if (!response.ok) throw new Error(`Billing request failed: ${response.status}`);
    state.invoices = (await response.json()).map(normalizeBillingInvoice);
    state.billingLoaded = true;
    if (state.view === 'billing') {
      const contentEl = document.getElementById('content');
      if (contentEl) contentEl.innerHTML = renderBillingLiveMarkup().replace(/\$/g, '₱');
      bindViewActions();
    }
  } catch (err) {
    state.billingLoaded = true;
    console.warn('Unable to load invoices from API, using local fallback:', err);
  }
}

async function loadLiveReports() {
  try {
    const response = await fetch('/api/reports/summary');
    if (!response.ok) throw new Error(`Reports request failed: ${response.status}`);
    state.reportSummary = await response.json();
    state.reportsLoaded = true;
    if (state.view === 'reports') {
      const contentEl = document.getElementById('content');
      if (contentEl) contentEl.innerHTML = renderReportsLiveMarkup().replace(/\$/g, '₱');
      bindViewActions();
    }
  } catch (err) {
    state.reportsLoaded = true;
    console.warn('Unable to load report summary:', err);
  }
}

function setEstimatedCostDefault() {
  const estimatedCostInput = document.getElementById('jobEstimatedCost');
  if (estimatedCostInput) {
    estimatedCostInput.value = '0.00';
  }
}

async function openModal(){
  document.getElementById('jobForm')?.reset();
  document.getElementById('modalBackdrop').classList.add('open');
  const dateInput = document.querySelector('#jobForm input[type="date"]');
  if (dateInput) dateInput.valueAsDate = new Date();
  setEstimatedCostDefault();
  await loadRepairOptions();
}

async function loadRepairOptions(selectedCustomerId = null, selectedDeviceId = null) {
  const customerSelect = document.getElementById('jobCustomer');
  const deviceSelect = document.getElementById('jobDevice');
  const technicianSelect = document.getElementById('jobTechnician');
  const addCustomerButton = document.getElementById('jobCustomerAdd');
  const addDeviceButton = document.getElementById('jobDeviceAdd');
  if (addCustomerButton) {
    addCustomerButton.remove();
  }
  if (!customerSelect || !deviceSelect) return;

  try {
    const response = await fetch('/api/repair-options');
    if (!response.ok) throw new Error(`Repair options request failed: ${response.status}`);
    const options = await response.json();
    customerSelect.innerHTML = '<option value="">Select customer</option>' + options.customers
      .map(customer => `<option value="${customer.customerId}">${escapeHtml(customer.name)}</option>`).join('') +
      '<option value="__add_new_customer__">＋ Add New Customer</option>';

    const updateDevices = () => {
      const customerId = Number(customerSelect.value);
      const devices = options.devices.filter(device => device.customerId === customerId);
      const hasDevices = devices.length > 0;
      deviceSelect.disabled = !customerId || !hasDevices;
      deviceSelect.innerHTML = `<option value="">${!customerId ? 'Select customer first' : hasDevices ? 'Select device' : 'No devices for this customer'}</option>` +
        devices.map(device => `<option value="${device.deviceId}">${escapeHtml(device.label.trim())}</option>`).join('');
      if (addDeviceButton) {
        addDeviceButton.style.display = !customerId || hasDevices ? 'none' : '';
      }
      if (selectedDeviceId && hasDevices) {
        deviceSelect.value = String(selectedDeviceId);
      }
    };
    customerSelect.onchange = async () => {
      if (customerSelect.value === '__add_new_customer__') {
        customerSelect.value = '';
        openCustomerModal(async (customer) => {
          const customerId = Number(customer?.id ?? customer?.customerId ?? 0);
          if (!customerId) return;
          await loadRepairOptions(customerId);
          customerSelect.value = String(customerId);
          setEstimatedCostDefault();
          updateDevices();
        });
        return;
      }
      updateDevices();
    };
    if (addDeviceButton) {
      addDeviceButton.onclick = () => {
        const customerId = Number(customerSelect.value);
        if (!customerId) {
          showActionMessage('Select a customer before adding a device.');
          return;
        }
        openDeviceDialog({ customerId, deviceType: '' }, { returnToRepairForm: true });
      };
    }
    if (selectedCustomerId) {
      customerSelect.value = String(selectedCustomerId);
    }
    setEstimatedCostDefault();
    updateDevices();
    if (Number(customerSelect.value) && selectedDeviceId) {
      deviceSelect.value = String(selectedDeviceId);
    }

    if (technicianSelect) {
      state.assignableTechnicians = options.technicians;
      technicianSelect.innerHTML = '<option value="">Unassigned</option>' + options.technicians
        .map(technician => `<option value="${technician.technicianId}">${escapeHtml(technician.name)}</option>`).join('');
    }
  } catch (error) {
    console.warn('Unable to load repair form options:', error);
    showActionMessage('Unable to load customers and devices for this repair.');
  }
}

function closeModal(){
  document.getElementById('modalBackdrop').classList.remove('open');
}

function openCustomerModal(onCustomerCreated = null){
  window.__repairCustomerCreatedCallback = typeof onCustomerCreated === 'function' ? onCustomerCreated : null;
  document.getElementById('customerModalBackdrop').classList.add('open');
  document.getElementById('customerForm').reset();
}

function closeCustomerModal(){
  window.__repairCustomerCreatedCallback = null;
  document.getElementById('customerModalBackdrop').classList.remove('open');
}

async function openAssignmentModal(jobId, technicianId = '') {
  if (!state.trackingLoaded) await loadLiveRepairTracking();
  if (!state.techniciansLoaded) await loadLiveTechnicians();
  const jobSelect = document.getElementById('assignmentJob');
  const technicianSelect = document.getElementById('assignmentTechnician');
  if (!jobSelect || !technicianSelect) return;

  const assignableJobs = state.jobs.filter(item => item.status !== 'Completed' && item.status !== 'Cancelled');
  jobSelect.innerHTML = '<option value="">Select a repair job</option>' + assignableJobs
    .map(item => `<option value="${item.repairJobId}">${escapeHtml(item.id)} · ${escapeHtml(item.customer)} · ${escapeHtml(item.device)}</option>`).join('');

  const job = state.jobs.find(item => item.repairJobId === Number(jobId) || item.id === jobId) || assignableJobs[0];
  if (!job) return;
  technicianSelect.innerHTML = '<option value="">Select a technician</option>' + state.technicians
    .filter(item => item.technicianId)
    .map(item => `<option value="${item.technicianId}">${escapeHtml(item.name)}</option>`).join('');
  state.assignmentJobId = job.repairJobId;
  jobSelect.value = String(job.repairJobId);
  technicianSelect.value = String(technicianId || job.technicianId || '');
  document.getElementById('assignmentModalBackdrop').classList.add('open');
}

function closeAssignmentModal() {
  document.getElementById('assignmentModalBackdrop').classList.remove('open');
  state.assignmentJobId = null;
}

function openArchiveConfirmation(customer) {
  return new Promise(resolve => {
    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop open';
    backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="archiveCustomerTitle"><div class="modal-header"><div><p class="eyebrow">Customer management</p><h2 id="archiveCustomerTitle">Archive customer</h2></div><button type="button" class="modal-close archive-cancel" aria-label="Close archive confirmation">×</button></div><div class="modal-body" style="padding:0 20px 12px;color:#53637a;font-size:14px;line-height:1.5;"><p>Archive <strong>${escapeHtml(customer.name)}</strong>? Their repair history will be kept.</p></div><div class="modal-actions"><button type="button" class="button button-ghost archive-cancel">Cancel</button><button type="button" class="button button-primary archive-confirm">Archive customer</button></div></section>`;
    document.body.appendChild(backdrop);
    const close = result => {
      backdrop.remove();
      resolve(result);
    };
    backdrop.querySelectorAll('.archive-cancel').forEach(button => button.onclick = () => close(false));
    backdrop.querySelector('.archive-confirm').onclick = () => close(true);
    backdrop.onclick = event => { if (event.target === backdrop) close(false); };
  });
}

function openArchiveUserConfirmation(userName) {
  return new Promise(resolve => {
    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop open';
    backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="archiveUserTitle"><div class="modal-header"><div><p class="eyebrow">User management</p><h2 id="archiveUserTitle">Archive user</h2></div><button type="button" class="modal-close archive-user-cancel" aria-label="Close archive confirmation">×</button></div><div class="modal-body" style="padding:0 20px 12px;color:#53637a;font-size:14px;line-height:1.5;"><p>Archive <strong>${escapeHtml(userName)}</strong>? This account will no longer be able to sign in.</p></div><div class="modal-actions"><button type="button" class="button button-ghost archive-user-cancel">Cancel</button><button type="button" class="button button-primary archive-user-confirm">Archive user</button></div></section>`;
    document.body.appendChild(backdrop);
    const close = result => {
      backdrop.remove();
      resolve(result);
    };
    backdrop.querySelectorAll('.archive-user-cancel').forEach(button => button.onclick = () => close(false));
    backdrop.querySelector('.archive-user-confirm').onclick = () => close(true);
    backdrop.onclick = event => { if (event.target === backdrop) close(false); };
  });
}

function openDeleteUserConfirmation(userName) {
  return new Promise(resolve => {
    const backdrop = document.createElement('div');
    backdrop.className = 'modal-backdrop open';
    backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="removeUserTitle"><div class="modal-header"><div><p class="eyebrow">User management</p><h2 id="removeUserTitle">Remove disabled account</h2></div><button type="button" class="modal-close remove-user-cancel" aria-label="Close remove confirmation">×</button></div><div class="modal-body" style="padding:0 20px 12px;color:#53637a;font-size:14px;line-height:1.5;"><p>Remove <strong>${escapeHtml(userName)}</strong> permanently? This action cannot be undone.</p></div><div class="modal-actions"><button type="button" class="button button-ghost remove-user-cancel">Cancel</button><button type="button" class="button button-primary remove-user-confirm">Remove account</button></div></section>`;
    document.body.appendChild(backdrop);
    const close = result => {
      backdrop.remove();
      resolve(result);
    };
    backdrop.querySelectorAll('.remove-user-cancel').forEach(button => button.onclick = () => close(false));
    backdrop.querySelector('.remove-user-confirm').onclick = () => close(true);
    backdrop.onclick = event => { if (event.target === backdrop) close(false); };
  });
}

function openTechnicianProfile(technician) {
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop open';
  backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="technicianProfileTitle"><div class="modal-header"><div><p class="eyebrow">Technician profile</p><h2 id="technicianProfileTitle">${technician.name}</h2></div><button type="button" class="modal-close technician-profile-close" aria-label="Close technician profile">×</button></div><div class="technician-profile-content"><p>${technician.specialty}</p><div class="metric-grid"><div class="metric"><label>Availability</label><strong>${technician.availability}</strong></div><div class="metric"><label>Current jobs</label><strong>${technician.currentJobs}</strong></div><div class="metric"><label>Completed</label><strong>${technician.completed}</strong></div><div class="metric"><label>Capacity</label><strong>${technician.currentJobs}/${technician.capacity}</strong></div></div></div><div class="modal-actions"><button type="button" class="button button-ghost technician-profile-close">Close</button><button type="button" class="button button-primary technician-profile-assign">Assign Job</button></div></section>`;
  document.body.appendChild(backdrop);
  const close = () => backdrop.remove();
  backdrop.querySelectorAll('.technician-profile-close').forEach(button => button.onclick = close);
  backdrop.onclick = event => { if (event.target === backdrop) close(); };
  backdrop.querySelector('.technician-profile-assign').onclick = () => {
    close();
    openAssignmentModal(null, technician.name);
  };
}

function bindViewActions(){
  document.querySelectorAll('[data-view-link]').forEach(btn => btn.onclick = () => {
    state.view = btn.dataset.viewLink;
    render();
  });

  const newJob = document.getElementById('newJob');
  if (newJob) newJob.onclick = openModal;

  const newCustomer = document.getElementById('newCustomer');
  if (newCustomer) newCustomer.onclick = openCustomerModal;

  const appearanceSelect = document.getElementById('appearanceSelect');
  if (appearanceSelect) appearanceSelect.onchange = () => {
    localStorage.setItem('techserve_appearance', appearanceSelect.value);
    applyAppearance();
  };

  document.querySelectorAll('.archive-customer').forEach(button => {
    button.onclick = async () => {
      const customerId = Number(button.dataset.customerId);
      const customer = state.customers.find(item => item.customerId === customerId);
      if (!customer || !await openArchiveConfirmation(customer)) return;

      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      try {
        const response = await fetch(`/api/customers/${customerId}/archive`, {
          method: 'POST',
          headers: { RequestVerificationToken: token }
        });
        const payload = await response.json().catch(() => ({}));
        if (!response.ok) {
          showActionMessage(payload.error || 'Customer could not be archived.');
          return;
        }
        customer.status = payload.status || 'Archived';
        state.customerStatusFilter = 'Active';
        const contentEl = document.getElementById('content');
        if (contentEl) contentEl.innerHTML = renderCustomers();
        bindViewActions();
        showActionMessage(`${customer.name} was archived.`);
      } catch (error) {
        console.warn('Unable to archive customer:', error);
        showActionMessage('Unable to archive the customer. Please try again.');
      }
    };
  });

  document.querySelectorAll('.customer-view-profile').forEach(button => {
    button.onclick = () => {
      const customerIndex = state.customers.findIndex(item => item.customerId === Number(button.dataset.customerId));
      if (customerIndex < 0) return;
      state.crmCustomerIndex = customerIndex;
      state.view = 'crm';
      render();
    };
  });

  document.querySelectorAll('[data-save-profile]').forEach(button => {
    button.onclick = async () => {
      const nameInput = document.getElementById('profileNameInput');
      const pictureInput = document.getElementById('profilePictureInput');
      const fullName = nameInput?.value.trim() || '';
      if (fullName.length < 2) {
        showActionMessage('Enter an admin name with at least 2 characters.');
        return;
      }

      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      try {
        const response = await fetch('/Auth/UpdateProfile', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
          body: JSON.stringify({ fullName })
        });
        const payload = await response.json().catch(() => ({}));
        if (!response.ok) {
          showActionMessage(payload.error || 'Your profile could not be updated.');
          return;
        }

        if (pictureInput?.files?.[0]) {
          const file = pictureInput.files[0];
          if (file.size > 2 * 1024 * 1024) {
            showActionMessage('Profile pictures must be 2 MB or smaller.');
            return;
          }
          const picture = await new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = () => resolve(reader.result);
            reader.onerror = reject;
            reader.readAsDataURL(file);
          });
          const username = window.__TECHSERVE_USER__?.username || state.currentUserRole;
          localStorage.setItem(`techserve_profile_picture_${username}`, picture);
          if (window.__TECHSERVE_USER__) window.__TECHSERVE_USER__.picture = picture;
        }
        if (window.__TECHSERVE_USER__) window.__TECHSERVE_USER__.name = payload.fullName;
        applyRoleProfile();
        showActionMessage(payload.message || 'Profile updated successfully.');
      } catch (error) {
        console.warn('Unable to update profile:', error);
        showActionMessage('Unable to update your profile. Please try again.');
      }
    };
  });

  document.querySelectorAll('[data-change-password]').forEach(button => {
    button.onclick = openPasswordChangeModal;
  });

  const addPart = document.querySelector('.inventory-actions .button-primary');
  if (addPart) addPart.onclick = () => openInventoryPartDialog();
  document.querySelectorAll('.edit-part').forEach(button => {
    button.onclick = () => {
      const part = state.parts.find(item => String(item.partId) === button.dataset.partId);
      if (part) openInventoryPartDialog(part);
    };
  });
  const inventorySearch = document.getElementById('inventorySearch');
  if (inventorySearch) inventorySearch.oninput = () => {
    const query = inventorySearch.value.trim().toLowerCase();
    document.querySelectorAll('.inventory-table tbody tr').forEach(row => {
      row.style.display = row.textContent.toLowerCase().includes(query) ? '' : 'none';
    });
  };
  document.querySelector('.inventory-stock-in')?.addEventListener('click', () => openStockAdjustmentDialog('IN'));
  document.querySelector('.inventory-stock-out')?.addEventListener('click', () => openStockAdjustmentDialog('OUT'));

  document.querySelectorAll('.edit-device').forEach(button => {
    button.onclick = () => {
      const device = state.devices.find(item => String(item.deviceId) === button.dataset.deviceId);
      if (device) openDeviceDialog(device);
    };
  });
  const deviceSearch = document.getElementById('deviceSearch');
  const deviceTypeFilter = document.getElementById('deviceTypeFilter');
  const filterDevices = () => {
    const query = deviceSearch?.value.trim().toLowerCase() || '';
    const selectedType = deviceTypeFilter?.value || 'All device types';
    document.querySelectorAll('#content tbody tr').forEach(row => {
      const matchesText = row.textContent.toLowerCase().includes(query);
      const matchesType = selectedType === 'All device types' || row.textContent.includes(selectedType);
      row.style.display = matchesText && matchesType ? '' : 'none';
    });
  };
  if (deviceSearch) deviceSearch.oninput = filterDevices;
  if (deviceTypeFilter) deviceTypeFilter.onchange = filterDevices;

  const newSupplier = document.getElementById('newSupplier');
  if (newSupplier) newSupplier.onclick = () => createActionFor('Add supplier');

  document.querySelectorAll('.supplier-view').forEach(button => {
    button.onclick = () => {
      const supplier = state.suppliers.find(item => String(item.supplierId) === String(button.dataset.supplierId));
      if (supplier) {
        showActionMessage(`${supplier.name} · ${supplier.contact} · ${supplier.email}`);
      }
    };
  });

  document.querySelectorAll('.tracking-card').forEach(card => {
    const open = () => {
      const job = state.trackingJobs?.find(item => String(item.repairJobId) === String(card.dataset.trackingId));
      if (job) openRepairTrackingDetails(job);
    };
    card.onclick = open;
    card.onkeydown = event => { if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); open(); } };
  });

  document.querySelectorAll('.view-repair-details').forEach(button => {
    button.onclick = () => {
      const job = state.trackingJobs?.find(item => String(item.repairJobId) === button.dataset.repairId);
      if (job) openRepairTrackingDetails(job);
    };
  });

  document.querySelectorAll('.view-technician-profile').forEach(button => {
    button.onclick = () => openTechnicianProfile({
      name: button.dataset.technician,
      specialty: button.dataset.specialty,
      availability: button.dataset.availability,
      currentJobs: button.dataset.currentJobs,
      completed: button.dataset.completed,
      capacity: button.dataset.capacity
    });
  });

  const supplierSearch = document.getElementById('supplierDashboardSearch');
  const supplierStatus = document.getElementById('supplierDashboardStatus');
  const filterSuppliers = () => {
    const query = supplierSearch?.value.trim().toLowerCase() || '';
    const selectedStatus = supplierStatus?.value || 'All statuses';
    document.querySelectorAll('#supplierDashboardTable tbody tr').forEach(row => {
      const matchesText = row.textContent.toLowerCase().includes(query);
      const matchesStatus = selectedStatus === 'All statuses' || row.textContent.includes(selectedStatus);
      row.style.display = matchesText && matchesStatus ? '' : 'none';
    });
  };
  if (supplierSearch) supplierSearch.oninput = filterSuppliers;
  if (supplierStatus) supplierStatus.onchange = filterSuppliers;
  const exportSupplierDashboard = document.getElementById('exportSupplierDashboard');
  if (exportSupplierDashboard) exportSupplierDashboard.onclick = exportCurrentTable;

  document.querySelectorAll('.assign-technician').forEach(button => {
    button.onclick = () => openAssignmentModal(button.dataset.jobId);
  });
  document.querySelectorAll('.assign-technician-card').forEach(button => {
    button.onclick = () => openAssignmentModal(null, button.dataset.technician);
  });
  document.querySelectorAll('.repair-actions button:not(.assign-technician):not(.view-repair-details)').forEach(button => {
    button.onclick = () => {
      const row = button.closest('tr');
      const job = state.jobs[[...document.querySelectorAll('.repair-actions button:not(.assign-technician):not(.view-repair-details)')].indexOf(button) / 3 | 0];
      if (!job || !row) return;
      const actionIndex = [...row.querySelectorAll('.repair-actions button:not(.assign-technician):not(.view-repair-details)')].indexOf(button);
      job.status = actionIndex === 0 ? 'Diagnosing' : actionIndex === 1 ? 'In Repair' : 'Completed';
      logActivity('repair_status_updated', `${job.id}: ${job.status}`);
      render();
    };
  });
  document.querySelectorAll('.record-payment').forEach(button => {
    button.onclick = () => {
      const invoice = state.invoices.find(item => String(item.invoiceId || item.id) === button.dataset.invoiceId);
      if (invoice) completePayment(invoice);
    };
  });
  document.querySelectorAll('.view-receipt').forEach(button => {
    button.onclick = () => {
      const invoice = state.invoices.find(item => String(item.invoiceId || item.id) === button.dataset.invoiceId);
      if (invoice) showReceipt(invoice, parseMoney(invoice.paid), 'Recorded payment');
    };
  });
  const assignTechnicianJob = document.getElementById('assignTechnicianJob');
  if (assignTechnicianJob) assignTechnicianJob.onclick = () => openAssignmentModal();

  const search = document.getElementById('tableSearch');
  if (search) search.oninput = () => {
    const value = search.value.toLowerCase();
    document.querySelectorAll('#content tbody tr').forEach(row => {
      row.style.display = row.textContent.toLowerCase().includes(value) ? '' : 'none';
    });
  };

  const filter = document.getElementById('statusFilter');
  if (filter) filter.onchange = () => {
    document.querySelectorAll('#repairTable tbody tr').forEach(row => {
      const matches = filter.value === 'All statuses' || row.textContent.includes(filter.value);
      row.style.display = matches ? '' : 'none';
    });
  };
  const repairFilter = document.getElementById('repairFilter');
  if (repairFilter) repairFilter.onclick = () => openActionDialog('Filter repair jobs', [{ name: 'status', label: 'Status', type: 'select', options: ['All statuses', 'Pending', 'Diagnosing', 'In Repair', 'Waiting for Parts', 'Ready for Pickup', 'Completed'] }], values => {
    document.querySelectorAll('#repairTable tbody tr').forEach(row => {
      row.style.display = values.status === 'All statuses' || row.dataset.repairStatus === values.status ? '' : 'none';
    });
  });
  const viewSelect = document.querySelector('#content select');
  if (viewSelect && state.view === 'customers') viewSelect.onchange = () => {
    const selected = viewSelect.value;
    state.customerStatusFilter = selected;
    const contentEl = document.getElementById('content');
    if (contentEl) contentEl.innerHTML = renderCustomers();
    bindViewActions();
  };
  if (viewSelect && state.view === 'billing') viewSelect.onchange = () => {
    const selected = viewSelect.value;
    document.querySelectorAll('#content tbody tr').forEach(row => { row.style.display = selected === 'All payment statuses' || row.textContent.includes(selected) ? '' : 'none'; });
  };
  document.querySelectorAll('.repair-tab').forEach(tab => tab.onclick = () => {
    document.querySelectorAll('.repair-tab').forEach(item => item.classList.remove('active'));
    tab.classList.add('active');
    const selected = tab.dataset.statusTab;
    document.querySelectorAll('#repairTable tbody tr').forEach(row => {
      row.style.display = selected === 'All' || row.dataset.repairStatus === selected ? '' : 'none';
    });
  });
  document.querySelectorAll('[data-crm-customer]').forEach(tab => tab.onclick = () => {
    state.crmCustomerIndex = Number(tab.dataset.crmCustomer);
    render();
  });
}

document.querySelectorAll('[data-crm-customer]').forEach(tab => tab.onclick = () => {
  state.crmCustomerIndex = Number(tab.dataset.crmCustomer);
  render();
});

const defaultAuthUsers = {
  admin: { password: 'admin123', role: 'ADMIN' },
  manager: { password: 'manager123', role: 'MANAGER' },
  employee: { password: 'staff123', role: 'STAFF' },
  staff: { password: 'staff123', role: 'STAFF' },
  technician: { password: 'technician123', role: 'TECHNICIAN' },
  inventory: { password: 'inventory123', role: 'INVENTORY' },
  billing: { password: 'billing123', role: 'BILLING' }
};

function loadAuthUsers() {
  const saved = JSON.parse(localStorage.getItem('techserve_users') || 'null');
  const merged = { ...(saved || {}), ...defaultAuthUsers };
  if (saved) {
    Object.entries(defaultAuthUsers).forEach(([key, value]) => {
      if (!saved[key]) {
        merged[key] = value;
      }
    });
  }
  return merged;
}

function saveAuthUsers(users) {
  localStorage.setItem('techserve_users', JSON.stringify(users));
}

function logActivity(action, details = '') {
  const session = JSON.parse(localStorage.getItem('techserve_session') || 'null');
  const entries = JSON.parse(localStorage.getItem('techserve_activity') || '[]');
  entries.unshift({
    action,
    details,
    user: session?.user || 'anonymous',
    role: session?.role || 'anonymous',
    at: new Date().toISOString()
  });
  localStorage.setItem('techserve_activity', JSON.stringify(entries.slice(0, 100)));
}

function syncAuthVisibility() {
  if (window.__TECHSERVE_USER__ && window.__TECHSERVE_USER__.role && roles[window.__TECHSERVE_USER__.role]) {
    state.currentUserRole = window.__TECHSERVE_USER__.role;
    state.view = initialViewFor(state.currentUserRole);
    const loginScreen = document.getElementById('loginScreen');
    const appShell = document.getElementById('appShell');
    if (loginScreen) loginScreen.classList.add('hidden');
    if (appShell) appShell.classList.remove('hidden');
    render();
    return;
  }

  const auth = JSON.parse(localStorage.getItem('techserve_session') || 'null');
  const sessionAge = auth?.createdAt ? Date.now() - auth.createdAt : Number.POSITIVE_INFINITY;
  const isLoggedIn = !!auth && sessionAge < 8 * 60 * 60 * 1000 && roles[auth.role];
  if (auth && !isLoggedIn) {
    localStorage.removeItem('techserve_session');
    logActivity('session_expired');
  }
  const loginScreen = document.getElementById('loginScreen');
  const appShell = document.getElementById('appShell');
  if (loginScreen) loginScreen.classList.toggle('hidden', isLoggedIn);
  if (appShell) appShell.classList.toggle('hidden', !isLoggedIn);
  if (isLoggedIn) {
    state.currentUserRole = auth.role;
    state.view = initialViewFor(auth.role);
    render();
  }
}

function loginUser(username, password) {
  const normalizedUsername = (username || '').trim().toLowerCase();
  const entry = loadAuthUsers()[normalizedUsername];
  const error = document.getElementById('loginError');
  if (!entry || entry.password !== password) {
    if (error) error.classList.remove('hidden');
    logActivity('login_failed', normalizedUsername || 'missing username');
    return;
  }
  localStorage.setItem('techserve_session', JSON.stringify({
    user: normalizedUsername,
    role: entry.role,
    createdAt: Date.now()
  }));
  logActivity('login_success');
  state.currentUserRole = entry.role;
  state.view = roles[entry.role].defaultView;
  if (error) error.classList.add('hidden');
  syncAuthVisibility();
}

function logoutUser() {
  logActivity('logout');
  localStorage.removeItem('techserve_session');
  syncAuthVisibility();
}

function confirmLogout(event) {
  if (event) event.preventDefault();

  const logoutUrl = event?.currentTarget?.getAttribute('href') || '/Auth/Logout';
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop open';
  backdrop.innerHTML = `
    <section class="modal" role="dialog" aria-modal="true" aria-labelledby="logoutConfirmTitle">
      <div class="modal-header">
        <div>
          <p class="eyebrow">Session</p>
          <h2 id="logoutConfirmTitle">Log out</h2>
        </div>
        <button type="button" class="modal-close logout-confirm-close" aria-label="Close">×</button>
      </div>
      <div class="modal-body" style="padding:0 20px 12px; color:#53637a; font-size:14px; line-height:1.5;">
        <p>Are you sure you want to log out?</p>
      </div>
      <div class="modal-actions" style="padding:0 20px 20px;">
        <button type="button" class="button button-ghost logout-confirm-cancel">Cancel</button>
        <button type="button" class="button button-primary logout-confirm-continue">Log out</button>
      </div>
    </section>
  `;

  document.body.appendChild(backdrop);
  const close = () => backdrop.remove();

  backdrop.querySelector('.logout-confirm-close')?.addEventListener('click', close);
  backdrop.querySelector('.logout-confirm-cancel')?.addEventListener('click', close);
  backdrop.addEventListener('click', event => {
    if (event.target === backdrop) close();
  });
  backdrop.querySelector('.logout-confirm-continue')?.addEventListener('click', () => {
    close();
    logoutUser();
    window.location.href = logoutUrl;
  });
}

document.querySelectorAll('.nav-item[data-view]').forEach(item => {
  item.onclick = () => {
    if (item.dataset.route) {
      window.location.href = item.dataset.route;
      return;
    }
    if (!document.getElementById('content')) {
      window.location.href = `/Admin/Dashboard?view=${encodeURIComponent(item.dataset.view)}`;
      return;
    }
    state.view = item.dataset.view;
    const sidebar = document.getElementById('sidebar');
    if (sidebar) sidebar.classList.remove('open');
    render();
  };
});

const menuToggle = document.getElementById('menuToggle');
if (menuToggle) {
  menuToggle.onclick = () => {
    const sidebar = document.getElementById('sidebar');
    if (sidebar) sidebar.classList.toggle('open');
  };
}

const modalClose = document.getElementById('modalClose');
if (modalClose) modalClose.onclick = closeModal;

const modalCancel = document.getElementById('modalCancel');
if (modalCancel) modalCancel.onclick = closeModal;

const modalBackdrop = document.getElementById('modalBackdrop');
if (modalBackdrop) {
  modalBackdrop.onclick = e => { if (e.target.id === 'modalBackdrop') closeModal(); };
}

const customerModalClose = document.getElementById('customerModalClose');
if (customerModalClose) customerModalClose.onclick = closeCustomerModal;

const customerModalCancel = document.getElementById('customerModalCancel');
if (customerModalCancel) customerModalCancel.onclick = closeCustomerModal;

const customerModalBackdrop = document.getElementById('customerModalBackdrop');
if (customerModalBackdrop) {
  customerModalBackdrop.onclick = e => {
    if (e.target.id === 'customerModalBackdrop') closeCustomerModal();
  };
}

const assignmentModalClose = document.getElementById('assignmentModalClose');
if (assignmentModalClose) assignmentModalClose.onclick = closeAssignmentModal;

const assignmentModalCancel = document.getElementById('assignmentModalCancel');
if (assignmentModalCancel) assignmentModalCancel.onclick = closeAssignmentModal;

const assignmentModalBackdrop = document.getElementById('assignmentModalBackdrop');
if (assignmentModalBackdrop) {
  assignmentModalBackdrop.onclick = e => {
    if (e.target.id === 'assignmentModalBackdrop') closeAssignmentModal();
  };
}

const passwordChangeModalClose = document.getElementById('passwordChangeModalClose');
if (passwordChangeModalClose) passwordChangeModalClose.onclick = closePasswordChangeModal;

const passwordChangeModalCancel = document.getElementById('passwordChangeModalCancel');
if (passwordChangeModalCancel) passwordChangeModalCancel.onclick = closePasswordChangeModal;

const passwordChangeModalBackdrop = document.getElementById('passwordChangeModalBackdrop');
if (passwordChangeModalBackdrop) {
  passwordChangeModalBackdrop.onclick = e => {
    if (e.target.id === 'passwordChangeModalBackdrop') closePasswordChangeModal();
  };
}

const passwordChangeForm = document.getElementById('passwordChangeForm');
if (passwordChangeForm) {
  passwordChangeForm.onsubmit = async (event) => {
    event.preventDefault();

    const currentPassword = document.getElementById('currentPassword')?.value ?? '';
    const newPassword = document.getElementById('newPassword')?.value ?? '';
    const confirmPassword = document.getElementById('confirmPassword')?.value ?? '';
    const message = document.getElementById('passwordChangeMessage');

    const showPasswordMessage = (text, isSuccess = false) => {
      if (!message) return;
      message.textContent = text;
      message.className = `form-message ${isSuccess ? 'success' : 'error'}`;
    };

    if (!currentPassword || !newPassword || !confirmPassword) {
      showPasswordMessage('Please complete all password fields.');
      return;
    }

    if (newPassword.length < 8) {
      showPasswordMessage('The new password must be at least 8 characters long.');
      return;
    }

    if (newPassword !== confirmPassword) {
      showPasswordMessage('The new passwords do not match.');
      return;
    }

    try {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const response = await fetch('/Auth/ChangePassword', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token
        },
        body: JSON.stringify({ currentPassword, newPassword, confirmPassword })
      });

      const payload = await response.json().catch(() => ({}));
      if (!response.ok) {
        showPasswordMessage(payload.error || 'The password could not be changed.');
        return;
      }

      showPasswordMessage(payload.message || 'Password changed successfully.', true);
      passwordChangeForm.reset();
      window.setTimeout(closePasswordChangeModal, 1000);
    } catch (error) {
      console.warn('Unable to change password:', error);
      showPasswordMessage('Unable to update your password. Please try again.');
    }
  };
}

const jobForm = document.getElementById('jobForm');
if (jobForm) {
  jobForm.onsubmit = async e => {
    e.preventDefault();
    const customerId = Number(document.getElementById('jobCustomer')?.value);
    const deviceId = Number(document.getElementById('jobDevice')?.value);
    const complaint = document.getElementById('jobComplaint')?.value.trim() || '';
    if (!customerId || !deviceId || !complaint) {
      showActionMessage('Please select a customer, device, and problem before creating the repair job.');
      return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    const request = {
      customerId,
      deviceId,
      technicianId: Number(document.getElementById('jobTechnician')?.value) || null,
      complaint,
      priority: document.getElementById('jobPriority')?.value || 'Normal',
      expectedCompletionDate: document.querySelector('#jobForm input[type="date"]')?.value || null,
      estimatedCost: Number(document.getElementById('jobEstimatedCost')?.value) || null
    };
    try {
      const response = await fetch('/api/repair-jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
        body: JSON.stringify(request)
      });
      if (!response.ok) {
        const error = await response.json().catch(() => ({ error: 'Repair job could not be saved.' }));
        showActionMessage(error.error || 'Repair job could not be saved.');
        return;
      }
      closeModal();
      state.view = 'repairs';
      state.trackingLoaded = false;
      await loadLiveRepairTracking();
    } catch (error) {
      console.warn('Unable to save repair job:', error);
      showActionMessage('Unable to save the repair job. Please try again.');
    }
  };
}

const customerForm = document.getElementById('customerForm');
if (customerForm) {
  customerForm.onsubmit = async e => {
    e.preventDefault();
    const name = document.getElementById('customerName')?.value.trim() || '';
    const contact = document.getElementById('customerContact')?.value.trim() || '';
    const email = document.getElementById('customerEmail')?.value.trim() || '';
    const address = document.getElementById('customerAddress')?.value.trim() || '';
    const emailEl = document.getElementById('customerEmail');
    if (name.length < 2 || contact.length < 7 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      if (emailEl) {
        emailEl.setCustomValidity('Enter a valid customer name, contact number, and email.');
        emailEl.reportValidity();
      }
      return;
    }
    if (emailEl) emailEl.setCustomValidity('');
    try {
      const response = await fetch('/api/customers', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, contact, email, address })
      });
      if (!response.ok) {
        const error = await response.json().catch(() => ({ error: 'The customer could not be saved.' }));
        if (emailEl) {
          emailEl.setCustomValidity(error.error || 'The customer could not be saved.');
          emailEl.reportValidity();
        }
        return;
      }
      const createdCustomer = await response.json();
      logActivity('customer_created', name);
      const repairCustomerCreatedCallback = window.__repairCustomerCreatedCallback;
      closeCustomerModal();

      if (typeof repairCustomerCreatedCallback === 'function') {
        await repairCustomerCreatedCallback(createdCustomer);
        window.__repairCustomerCreatedCallback = null;
      } else {
        state.view = 'customers';
        render();
      }
    } catch (err) {
      console.warn('Saving customer locally:', err);
      showActionMessage('Unable to save the customer. Please try again.');
    }
  };
}

const assignmentForm = document.getElementById('assignmentForm');
if (assignmentForm) {
  assignmentForm.onsubmit = async e => {
    e.preventDefault();
    const repairJobId = Number(document.getElementById('assignmentJob')?.value);
    const technicianId = Number(document.getElementById('assignmentTechnician')?.value);
    if (!repairJobId || !technicianId) return;
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
    try {
      const response = await fetch(`/api/repair-jobs/${repairJobId}/technician`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
        body: JSON.stringify({ technicianId })
      });
      if (!response.ok) {
        const error = await response.json().catch(() => ({ error: 'Technician assignment could not be saved.' }));
        showActionMessage(error.error || 'Technician assignment could not be saved.');
        return;
      }
      closeAssignmentModal();
      state.view = 'repairs';
      state.trackingLoaded = false;
      await loadLiveRepairTracking();
    } catch (error) {
      console.warn('Unable to save technician assignment:', error);
      showActionMessage('Unable to save the technician assignment. Please try again.');
    }
  };
}

const notificationButton = document.querySelector('.notification');
if (notificationButton) {
  notificationButton.onclick = () => {
    let panel = document.getElementById('notificationPanel');
    if (panel) {
      panel.remove();
      return;
    }

    panel = document.createElement('section');
    panel.id = 'notificationPanel';
    panel.setAttribute('role', 'status');
    Object.assign(panel.style, {
      position: 'fixed', top: '64px', right: '24px', zIndex: '30', width: 'min(340px, calc(100vw - 32px))',
      background: '#fff', border: '1px solid #e8edf3', borderRadius: '10px', padding: '16px',
      boxShadow: '0 14px 35px rgba(30,53,84,.16)'
    });
    panel.innerHTML = '<div style="display:flex;justify-content:space-between;gap:12px;align-items:center"><strong>Notifications</strong><button type="button" class="notification-clear" style="border:0;background:transparent;color:#3268e8;cursor:pointer">Mark read</button></div><p style="margin:12px 0 0;color:#53637a;font-size:13px">3 repairs need attention today.</p><p style="margin:8px 0 0;color:#53637a;font-size:13px">Inventory item P-003 is out of stock.</p>';
    document.body.appendChild(panel);
    notificationButton.querySelector('i')?.remove();
    panel.querySelector('.notification-clear').onclick = () => panel.remove();
  };
}

const globalSearch = document.getElementById('globalSearch');
if (globalSearch) {
  globalSearch.oninput = e => {
    if (e.target.value.trim() && state.view !== 'repairs') {
      state.view = 'repairs';
      render();
      const tableSearch = document.getElementById('tableSearch');
      if (tableSearch) {
        tableSearch.value = e.target.value;
        tableSearch.dispatchEvent(new Event('input'));
      }
    }
  };
}

const loginForm = document.getElementById('loginForm');
if (loginForm) {
  loginForm.onsubmit = e => {
    e.preventDefault();
    const user = document.getElementById('loginUser')?.value.trim() || '';
    const pass = document.getElementById('loginPassword')?.value || '';
    loginUser(user, pass);
  };
}

const logoutBtn = document.getElementById('logoutButton');
if (logoutBtn) logoutBtn.onclick = confirmLogout;

const inviteUserModal = document.getElementById('inviteUserModal');
const openInviteUser = document.getElementById('openInviteUser');
const closeInviteUser = document.getElementById('closeInviteUser');
const cancelInviteUser = document.getElementById('cancelInviteUser');
const setInviteUserModalOpen = isOpen => {
  if (!inviteUserModal) return;
  inviteUserModal.hidden = !isOpen;
  inviteUserModal.classList.toggle('open', isOpen);
};
if (openInviteUser) openInviteUser.onclick = () => setInviteUserModalOpen(true);
if (closeInviteUser) closeInviteUser.onclick = () => setInviteUserModalOpen(false);
if (cancelInviteUser) cancelInviteUser.onclick = () => setInviteUserModalOpen(false);
if (inviteUserModal) {
  inviteUserModal.addEventListener('click', event => {
    if (event.target === inviteUserModal) setInviteUserModalOpen(false);
  });
}
document.querySelectorAll('.user-archive-form').forEach(form => {
  form.onsubmit = async event => {
    event.preventDefault();
    if (await openArchiveUserConfirmation(form.dataset.userName || 'this user')) {
      HTMLFormElement.prototype.submit.call(form);
    }
  };
});
document.querySelectorAll('.user-delete-form').forEach(form => {
  form.onsubmit = async event => {
    event.preventDefault();
    if (await openDeleteUserConfirmation(form.dataset.userName || 'this account')) {
      HTMLFormElement.prototype.submit.call(form);
    }
  };
});
if (window.location.hash === '#user-management') {
  window.setTimeout(() => document.getElementById('user-management')?.scrollIntoView({ block: 'start' }), 0);
}

function showActionMessage(message) {
  let toast = document.getElementById('actionToast');
  if (!toast) {
    toast = document.createElement('div');
    toast.id = 'actionToast';
    toast.className = 'action-toast';
    Object.assign(toast.style, {
      position: 'fixed', right: '24px', bottom: '24px', zIndex: '20',
      padding: '12px 16px', borderRadius: '8px', background: '#17243a',
      color: '#fff', boxShadow: '0 8px 24px rgba(23,36,58,.25)',
      fontSize: '13px', opacity: '0', pointerEvents: 'none', transition: 'opacity .2s'
    });
    document.body.appendChild(toast);
  }
  toast.textContent = message;
  toast.style.opacity = '1';
  clearTimeout(showActionMessage.timer);
  showActionMessage.timer = setTimeout(() => { toast.style.opacity = '0'; }, 2600);
}

function openActionDialog(title, fields, onSubmit) {
  const backdrop = document.createElement('div');
  backdrop.className = 'modal-backdrop open';
  backdrop.innerHTML = `<section class="modal" role="dialog" aria-modal="true" aria-labelledby="actionDialogTitle"><div class="modal-header"><div><p class="eyebrow">TechServe action</p><h2 id="actionDialogTitle">${escapeHtml(title)}</h2></div><button type="button" class="modal-close action-dialog-close" aria-label="Close">×</button></div><form class="action-dialog-form"><div class="form-grid">${fields.map(field => `<label class="${field.wide ? 'wide' : ''}">${escapeHtml(field.label)}${field.type === 'select' ? `<select name="${field.name}" ${field.required ? 'required' : ''}>${field.options.map(option => { const value = typeof option === 'string' ? option : option.value; const label = typeof option === 'string' ? option : option.label; return `<option value="${escapeHtml(value)}" ${String(value) === String(field.value ?? '') ? 'selected' : ''}>${escapeHtml(label)}</option>`; }).join('')}</select>` : `<input name="${field.name}" type="${field.type || 'text'}" ${field.required ? 'required' : ''} ${field.maxLength ? `maxlength="${field.maxLength}"` : ''} value="${escapeHtml(field.value || '')}" placeholder="${escapeHtml(field.placeholder || '')}">`}</label>`).join('')}</div><div class="modal-actions"><button type="button" class="button button-ghost action-dialog-cancel">Cancel</button><button type="submit" class="button button-primary">Save</button></div></form></section>`;
  document.body.appendChild(backdrop);
  const close = () => backdrop.remove();
  backdrop.querySelector('.action-dialog-close').onclick = close;
  backdrop.querySelector('.action-dialog-cancel').onclick = close;
  backdrop.onclick = event => { if (event.target === backdrop) close(); };
  backdrop.querySelector('form').onsubmit = async event => {
    event.preventDefault();
    const result = await onSubmit(Object.fromEntries(new FormData(event.target)));
    if (result === false) return;
    close();
  };
}

async function openDeviceDialog(device = null, { returnToRepairForm = false } = {}) {
  try {
    const isEditing = Boolean(device?.deviceId);
    const response = await fetch('/api/customers');
    if (!response.ok) throw new Error(`Customer request failed: ${response.status}`);
    const customers = await response.json();
    const fields = [
      { name: 'customerId', label: 'Customer', type: 'select', options: customers.map(customer => ({ value: customer.id, label: customer.name })), value: device?.customerId, required: true },
      { name: 'deviceType', label: 'Device type', value: device?.deviceType, required: true, maxLength: 60 },
      { name: 'brand', label: 'Brand', value: device?.brand, maxLength: 60 },
      { name: 'model', label: 'Model', value: device?.model, maxLength: 80 },
      { name: 'serialNumber', label: 'Serial number', value: device?.serialNumber, maxLength: 120 },
      { name: 'operatingSystem', label: 'Operating system', value: device?.operatingSystem, maxLength: 80 },
      { name: 'condition', label: 'Condition', value: device?.condition, maxLength: 40 },
      { name: 'accessories', label: 'Accessories', value: device?.accessories, maxLength: 300 }
    ];
    openActionDialog(isEditing ? 'Edit device' : 'Register device', fields, async values => {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      const request = {
        customerId: Number(values.customerId),
        deviceType: values.deviceType,
        brand: values.brand,
        model: values.model,
        serialNumber: values.serialNumber,
        operatingSystem: values.operatingSystem,
        condition: values.condition,
        accessories: values.accessories
      };
      try {
        const saveResponse = await fetch(isEditing ? `/api/devices/${device.deviceId}` : '/api/devices', {
          method: isEditing ? 'PUT' : 'POST',
          headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
          body: JSON.stringify(request)
        });
        if (!saveResponse.ok) {
          const error = await saveResponse.json().catch(() => ({ error: 'Device could not be saved.' }));
          showActionMessage(error.error || 'Device could not be saved.');
          return false;
        }

        const savedDevice = await saveResponse.json().catch(() => null);
        const repairCustomerSelect = document.getElementById('jobCustomer');
        const repairDeviceSelect = document.getElementById('jobDevice');
        if (!isEditing && returnToRepairForm && repairCustomerSelect && repairDeviceSelect && Number(repairCustomerSelect.value) === Number(request.customerId)) {
          await loadRepairOptions(Number(repairCustomerSelect.value), savedDevice?.deviceId ?? null);
          repairDeviceSelect.value = String(savedDevice?.deviceId ?? (repairDeviceSelect.value || ''));
        }

        if (!returnToRepairForm) {
          state.view = 'devices';
          state.devicesLoaded = false;
          await loadLiveDevices();
        }
        showActionMessage(isEditing ? 'Device updated.' : 'Device registered.');
        return true;
      } catch (error) {
        console.warn('Unable to save device:', error);
        showActionMessage('Unable to save the device. Please try again.');
        return false;
      }
    });
  } catch (error) {
    console.warn('Unable to load customers for device registration:', error);
    showActionMessage('Unable to load customers for device registration.');
  }
}

function createActionFor(text) {
  const actions = {
    'Add Part': { fields: [{ name: 'name', label: 'Part name', required: true }, { name: 'category', label: 'Category', required: true }, { name: 'quantity', label: 'Quantity', type: 'number', required: true }], message: values => `${values.name} added with ${values.quantity} in stock.` },
    'Create invoice': { fields: [{ name: 'customer', label: 'Customer', required: true }, { name: 'amount', label: 'Amount', type: 'number', required: true }, { name: 'job', label: 'Job order', required: true }], message: values => `Invoice created for ${values.customer}.` },
    'Invite member': { fields: [{ name: 'name', label: 'Full name', required: true }, { name: 'email', label: 'Email', type: 'email', required: true }, { name: 'role', label: 'Role', type: 'select', options: ['MANAGER', 'STAFF', 'TECHNICIAN', 'INVENTORY', 'BILLING'], required: true }], message: values => `Invitation sent to ${values.email}.` }
  };
  if (text === 'Register device') {
    openDeviceDialog();
    return true;
  }
  if (text === 'Add supplier') {
    openActionDialog('Add supplier', [
      { name: 'name', label: 'Supplier name', required: true },
      { name: 'category', label: 'Category', required: true },
      { name: 'contactPerson', label: 'Contact person' },
      { name: 'contactEmail', label: 'Email', type: 'email' },
      { name: 'phone', label: 'Phone' },
      { name: 'address', label: 'Address', wide: true }
    ], async values => {
      const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
      try {
        const response = await fetch('/api/suppliers', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json', RequestVerificationToken: token },
          body: JSON.stringify({ ...values, status: 'Active', paymentTerms: 'Net 30', leadTime: '3-5 Days' })
        });
        if (!response.ok) {
          const error = await response.json().catch(() => ({ error: 'Supplier could not be saved.' }));
          showActionMessage(error.error || 'Supplier could not be saved.');
          return false;
        }
        state.view = 'suppliers';
        state.suppliersLoaded = false;
        await loadLiveSuppliers();
        showActionMessage(`${values.name} added to suppliers.`);
        return true;
      } catch (error) {
        console.warn('Unable to save supplier:', error);
        showActionMessage('Unable to save the supplier. Please try again.');
        return false;
      }
    });
    return true;
  }
  const action = actions[text];
  if (!action) return false;
  openActionDialog(text, action.fields, values => {
    if (text === 'Create invoice') {
      const numericId = Math.max(...state.invoices.map(invoice => Number(String(invoice.invoiceId ?? invoice.id).replace(/\D/g, '') || 0)), 0) + 1;
      const amount = Number(values.amount) || 0;
      const invoice = {
        invoiceId: numericId,
        id: `INV-${String(numericId).padStart(4, '0')}`,
        job: values.job,
        customer: values.customer,
        amount: money(amount),
        paid: money(0),
        status: amount > 0 ? 'Unpaid' : 'Paid',
        date: new Date().toLocaleDateString('en-PH', { month: 'short', day: '2-digit', year: 'numeric' })
      };
      state.invoices.unshift(invoice);
      showActionMessage(`Invoice ${invoice.id} created for ${values.customer}.`);
      state.view = 'billing';
      render();
      return;
    }

    logActivity(text.toLowerCase().replace(/\s+/g, '_'), JSON.stringify(values));
    showActionMessage(action.message(values));
  });
  return true;
}

function exportCurrentTable() {
  const table = document.querySelector('#content table');
  if (!table) {
    showActionMessage('There is no table to export on this page.');
    return;
  }
  const rows = [...table.querySelectorAll('tr')].map(row => [...row.children].map(cell => {
    const value = cell.textContent.trim().replace(/"/g, '""');
    return `"${value}"`;
  }).join(','));
  const blob = new Blob([rows.join('\n')], { type: 'text/csv;charset=utf-8' });
  const link = document.createElement('a');
  link.href = URL.createObjectURL(blob);
  link.download = `techserve-${state.view}.csv`;
  link.click();
  URL.revokeObjectURL(link.href);
  showActionMessage('CSV export downloaded.');
}

document.addEventListener('click', event => {
  const button = event.target.closest('button');
  if (!button || button.onclick || button.closest('#modalBackdrop, #customerModalBackdrop, #assignmentModalBackdrop')) return;
  const text = button.textContent.trim().replace(/^[^A-Za-z0-9]+/, '').replace(/\s+/g, ' ');

  if (/Export CSV|Export/.test(text)) {
    exportCurrentTable();
  } else if (/Print report/.test(text)) {
    window.print();
  } else if (/^view profile$/i.test(text)) {
    state.view = 'crm';
    render();
  } else if (/^New Job$/.test(text)) {
    openModal();
  } else if (/^Add customer$/.test(text)) {
    openCustomerModal();
  } else if (/^Register device$|^Add Technician$|^Add Part$|^Create invoice$|^Invite member$/.test(text)) {
    createActionFor(text);
  } else if (/^Save changes$/.test(text)) {
    const settings = [...document.querySelectorAll('#content input, #content select')].map(input => [input.previousSibling?.textContent?.trim() || input.name, input.value]);
    localStorage.setItem('techserve_settings', JSON.stringify(settings));
    logActivity('settings_saved');
    showActionMessage('Workspace settings saved.');
  } else if (/^Edit$/.test(text)) {
    openActionDialog('Edit customer', [{ name: 'name', label: 'Full name', required: true }, { name: 'contact', label: 'Contact number', required: true }, { name: 'email', label: 'Email', type: 'email', required: true }], values => {
      showActionMessage(`${values.name} updated.`);
      logActivity('customer_updated', values.name);
    });
  } else if (/^View all$|^View all jobs/.test(text)) {
    state.view = 'repairs';
    render();
  } else if (/^▣$|^◉$|^／$/.test(text)) {
    showActionMessage('Repair action selected.');
  } else if (/^▽ Filter$/.test(text)) {
    showActionMessage('Use the search field and status tabs to filter repair jobs.');
  } else if (button.matches('.help')) {
    showActionMessage('Help center: choose a workspace section to get started.');
  }
});

syncAuthVisibility();
