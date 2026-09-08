const roles = {
  ADMIN: {
    initials: 'AD',
    profile: { name: 'Alex Morgan', role: 'Administrator' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'customers', 'devices', 'inventory', 'billing', 'crm', 'reports', 'settings']
  },
  MANAGER: {
    initials: 'MG',
    profile: { name: 'Maya Lopez', role: 'Manager' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'customers', 'devices', 'inventory', 'billing', 'crm', 'reports']
  },
  TECHNICIAN: {
    initials: 'TC',
    profile: { name: 'Noah Williams', role: 'Technician' },
    defaultView: 'repairs',
    nav: ['overview', 'repairs', 'devices', 'crm']
  },
  STAFF: {
    initials: 'ST',
    profile: { name: 'Jamie Cole', role: 'Staff' },
    defaultView: 'customers',
    nav: ['overview', 'customers', 'devices', 'repairs', 'billing', 'crm']
  },
  INVENTORY: {
    initials: 'IN',
    profile: { name: 'Riley Chen', role: 'Inventory' },
    defaultView: 'inventory',
    nav: ['overview', 'inventory', 'devices', 'repairs', 'reports']
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
  invoices: [
    {id:'INV-0871',job:'JOB-1045',customer:'Jordan Lee',amount:'$185.00',paid:'$0.00',status:'Unpaid',date:'Sep 08, 2024'},
    {id:'INV-0870',job:'JOB-1044',customer:'Maya Thompson',amount:'$324.50',paid:'$324.50',status:'Paid',date:'Sep 08, 2024'},
    {id:'INV-0869',job:'JOB-1043',customer:'Owen Brooks',amount:'$96.00',paid:'$50.00',status:'Partially Paid',date:'Sep 07, 2024'},
  ]
};

const content = document.getElementById('content');
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
  'Cancelled':'status-red',
  'Returning':'status-green',
  'Active':'status-blue'
}[s] || 'status-gray');

const money = value => `$${Number(value).toLocaleString('en-US',{minimumFractionDigits:2,maximumFractionDigits:2})}`;
const initials = name => name.split(' ').map(v=>v[0]).join('').slice(0,2);
const userProfile = () => roles[state.currentUserRole].profile;

function layout(title, subtitle, action = '') {
  return `<div class="page-heading"><div><p class="eyebrow">TechServe workspace</p><h1>${title}</h1><p>${subtitle}</p></div>${action}</div>`;
}
function person(name, code = initials(name)) { return `<span class="person"><span class="mini-avatar">${code}</span>${name}</span>`; }
function status(s) { return `<span class="status ${statusClass(s)}">${s}</span>`; }

function applyRoleProfile() {
  const role = roles[state.currentUserRole];
  const profile = role.profile;
  document.getElementById('workspaceInitials').textContent = role.initials;
  document.getElementById('workspaceRole').textContent = state.currentUserRole;
  document.getElementById('profileInitials').textContent = profile.name.split(' ').map(x => x[0]).join('').slice(0,2);
  document.getElementById('profileName').textContent = profile.name;
  document.getElementById('profileRole').textContent = profile.role;
  document.getElementById('roleSelector').value = state.currentUserRole;
}

function applyNavigationAccess() {
  const allowed = new Set(roles[state.currentUserRole].nav);
  document.querySelectorAll('.nav-item[data-view]').forEach((item) => {
    item.style.display = allowed.has(item.dataset.view) ? '' : 'none';
    item.classList.toggle('active', item.dataset.view === state.view);
  });

  const currentRoleViews = roles[state.currentUserRole].nav;
  if (!currentRoleViews.includes(state.view)) {
    state.view = currentRoleViews[0];
  }
}

function renderOverview() {
  const jobRows = state.jobs.slice(0,4).map(j => `<tr><td><b>${j.id}</b></td><td>${person(j.customer,j.initials)}</td><td>${j.device}</td><td>${j.tech}</td><td>${status(j.status)}</td><td><span class="tag">${j.priority}</span></td></tr>`).join('');
  const action = ['ADMIN','MANAGER','STAFF'].includes(state.currentUserRole)
    ? '<button class="button button-primary" id="newJob">＋ New repair job</button>'
    : '';
  return layout('Good morning, ' + userProfile().name.split(' ')[0] + ' 👋','Here’s what’s happening at your repair shop today.', action) +
    `<div class="stat-grid">
      <div class="stat-card"><div class="stat-top">Total customers <span class="stat-icon i-blue">♙</span></div><h2>1,284</h2><span class="trend">↗ 12.8% <span>vs last month</span></span></div>
      <div class="stat-card"><div class="stat-top">Active repair jobs <span class="stat-icon i-orange">▣</span></div><h2>38</h2><span class="trend">↗ 8.4% <span>vs last month</span></span></div>
      <div class="stat-card"><div class="stat-top">Completed repairs <span class="stat-icon i-green">✓</span></div><h2>126</h2><span class="trend">↗ 15.2% <span>vs last month</span></span></div>
      <div class="stat-card"><div class="stat-top">Total revenue <span class="stat-icon i-blue">$</span></div><h2>$24,680</h2><span class="trend">↗ 18.6% <span>vs last month</span></span></div>
    </div>
    <div class="dashboard-grid">
      <section class="panel"><div class="panel-header"><div><h3>Revenue overview</h3><p>Monthly revenue from repair jobs and parts</p></div><div class="chart-legend"><span class="legend"><i class="dot" style="background:#3268e8"></i>Revenue</span><span class="legend"><i class="dot" style="background:#9eb9f8"></i>Last year</span><select class="select" style="height:28px;font-size:10px"><option>Last 6 months</option></select></div></div>
        <div class="chart"><div class="chart-grid"><span><em>$30k</em></span><span><em>$20k</em></span><span><em>$10k</em></span><span><em>$0</em></span></div><svg viewBox="0 0 600 170" preserveAspectRatio="none"><path d="M0 140 C45 130 60 110 100 119 S160 93 200 101 S250 67 300 79 S350 48 400 57 S450 35 500 42 S550 13 600 23" fill="none" stroke="#3268e8" stroke-width="3"/><path d="M0 140 C45 130 60 110 100 119 S160 93 200 101 S250 67 300 79 S350 48 400 57 S450 35 500 42 S550 13 600 23 V170 H0Z" fill="#3268e8" opacity=".07"/><path d="M0 151 C45 143 60 138 100 145 S160 119 200 130 S250 110 300 115 S350 98 400 102 S450 84 500 93 S550 69 600 76" fill="none" stroke="#a9bff3" stroke-width="2" stroke-dasharray="4 4"/></svg><div class="x-axis"><span>Apr</span><span>May</span><span>Jun</span><span>Jul</span><span>Aug</span><span>Sep</span></div></div>
      </section>
      <section class="panel"><div class="panel-header"><div><h3>Repair job status</h3><p>38 active jobs across the shop</p></div><button class="panel-link" data-view-link="repairs">View all</button></div><div class="donut-wrap"><div class="donut"></div><div class="legend-list"><span class="legend"><i class="dot" style="background:#3268e8"></i>In repair <b>13</b></span><span class="legend"><i class="dot" style="background:#54bd93"></i>Completed <b>9</b></span><span class="legend"><i class="dot" style="background:#f4a95b"></i>Diagnosing <b>8</b></span><span class="legend"><i class="dot" style="background:#d6ddea"></i>Other <b>8</b></span></div></div></section>
    </div>
    <section class="panel recent"><div class="panel-header"><div><h3>Recent repair jobs</h3><p>Latest activity across your repair floor</p></div><button class="panel-link" data-view-link="repairs">View all jobs →</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Job order</th><th>Customer</th><th>Device</th><th>Technician</th><th>Status</th><th>Priority</th></tr></thead><tbody>${jobRows}</tbody></table></div></section>`;
}

function renderRepairs() {
  const canCreate = ['ADMIN','MANAGER','STAFF'].includes(state.currentUserRole);
  return layout('Repair jobs','Manage every repair from intake to release.', canCreate ? '<button class="button button-primary" id="newJob">＋ New repair job</button>' : '') +
    `<section class="panel"><div class="toolbar"><input class="table-search" id="tableSearch" placeholder="⌕  Search job, customer or device..."><select class="select" id="statusFilter"><option>All statuses</option><option>In Repair</option><option>Diagnosing</option><option>Waiting for Parts</option><option>Testing</option><option>Completed</option></select><select class="select"><option>All technicians</option><option>Noah Williams</option><option>Sofia Patel</option><option>Liam Chen</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table" id="repairTable"><thead><tr><th>Job order</th><th>Customer</th><th>Device / complaint</th><th>Technician</th><th>Received</th><th>Status</th><th>Progress</th><th></th></tr></thead><tbody>${state.jobs.map(j=>`<tr><td><b>${j.id}</b><small style="display:block;color:#9aa7b7;margin-top:4px">${j.priority} priority</small></td><td>${person(j.customer,j.initials)}</td><td><b>${j.device}</b><small style="display:block;color:#8290a1;margin-top:4px">${j.issue}</small></td><td>${j.tech}</td><td>${j.received}</td><td>${status(j.status)}</td><td><span class="progress-bar"><i style="width:${j.progress}%"></i></span>${j.progress}%</td><td><button class="panel-link">•••</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderCustomers() {
  return layout('Customers','Keep customer profiles, contact details and repair history in one place.','<button class="button button-primary">＋ Add customer</button>')+
    `<section class="panel"><div class="toolbar"><input class="table-search" id="tableSearch" placeholder="⌕  Search customers..."><select class="select"><option>All customers</option><option>Returning customers</option><option>Active</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Customer</th><th>Customer ID</th><th>Contact</th><th>Email</th><th>Repair jobs</th><th>Status</th><th></th></tr></thead><tbody>${state.customers.map(c=>`<tr><td>${person(c.name)}</td><td>${c.id}</td><td>${c.contact}</td><td>${c.email}</td><td><b>${c.jobs}</b></td><td>${status(c.status)}</td><td><button class="panel-link">View profile</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderDevices() {
  const devices=[['DEV-0084','Priya Nair','Dell XPS 15','DX15-44A91','Windows 11','Good'],['DEV-0083','Marcus Reed','MacBook Pro 14"','C02ZQ0ABMD6T','macOS Sonoma','Good'],['DEV-0082','Elena Cruz','HP EliteBook 840','5CD3412K8L','Windows 10','Fair'],['DEV-0081','Jordan Lee','Lenovo ThinkPad T14','PF3X9L7M','Windows 11','Good']];
  return layout('Devices','A complete view of every device registered with your shop.','<button class="button button-primary">＋ Register device</button>')+`<section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search device, serial or customer..."><select class="select"><option>All device types</option><option>Laptop</option><option>Desktop</option><option>MacBook</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Device ID</th><th>Customer</th><th>Device</th><th>Serial number</th><th>Operating system</th><th>Condition</th><th>Repair history</th></tr></thead><tbody>${devices.map(d=>`<tr><td><b>${d[0]}</b></td><td>${person(d[1])}</td><td><b>${d[2]}</b></td><td class="tag">${d[3]}</td><td>${d[4]}</td><td>${status(d[5]==='Fair'?'Diagnosing':'Completed')}</td><td><button class="panel-link">2 jobs →</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderInventory() {
  return layout('Parts inventory','Track stock levels, suppliers and parts used in active repairs.','<button class="button button-primary">＋ Add part</button>')+
    `<div class="metric-grid"><div class="panel metric"><label>Inventory value</label><strong>$48,290</strong><small>↗ 6.2% this month</small></div><div class="panel metric"><label>Low stock items</label><strong style="color:var(--orange)">3</strong><small style="color:var(--orange)">Needs attention</small></div><div class="panel metric"><label>Out of stock</label><strong style="color:var(--red)">1</strong><small style="color:var(--red)">Reorder now</small></div></div><section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search part or supplier..."><select class="select"><option>All categories</option><option>Power</option><option>Storage</option><option>Display</option><option>Memory</option></select><span class="spacer"></span><button class="button button-light">＋ Stock in</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Part</th><th>Category</th><th>Supplier</th><th>In stock</th><th>Reorder level</th><th>Unit cost</th><th>Stock status</th><th></th></tr></thead><tbody>${state.parts.map(p=>`<tr><td><b>${p.name}</b><small style="display:block;color:#9aa7b7;margin-top:4px">${p.id}</small></td><td>${p.category}</td><td>${p.supplier}</td><td><b>${p.stock}</b></td><td>${p.reorder}</td><td>${p.cost}</td><td>${p.stock===0?status('Unpaid'):p.stock<=p.reorder?status('Waiting for Parts'):status('Completed')}</td><td><button class="panel-link">•••</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderBilling() {
  return layout('Billing & invoices','Create invoices, record payments and keep balances clear.','<button class="button button-primary">＋ Create invoice</button>')+
    `<div class="metric-grid"><div class="panel metric"><label>Collected this month</label><strong>$18,420</strong><small>↗ 18.6% vs last month</small></div><div class="panel metric"><label>Outstanding balance</label><strong style="color:var(--orange)">$4,875</strong><small style="color:var(--orange)">12 invoices unpaid</small></div><div class="panel metric"><label>Average invoice</label><strong>$194.60</strong><small>Across 126 completed repairs</small></div></div><section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search invoice or customer..."><select class="select"><option>All payment statuses</option><option>Paid</option><option>Partially Paid</option><option>Unpaid</option></select><span class="spacer"></span><button class="button button-ghost">▣ Print report</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Invoice</th><th>Job order</th><th>Customer</th><th>Amount</th><th>Paid</th><th>Balance</th><th>Status</th><th>Date</th></tr></thead><tbody>${state.invoices.map(i=>{ const amount=parseFloat(i.amount.replace(/[$,]/g,'')); const paid=parseFloat(i.paid.replace(/[$,]/g,'')); const balance=amount-paid; return `<tr><td><b>${i.id}</b></td><td>${i.job}</td><td>${person(i.customer)}</td><td><b>${i.amount}</b></td><td>${i.paid}</td><td><b>${money(balance)}</b></td><td>${status(i.status)}</td><td>${i.date}</td></tr>`; }).join('')}</tbody></table></div></section>`;
}

function renderCrm() {
  return layout('Customer CRM','Build stronger relationships with notes, reminders and activity history.','<button class="button button-primary">＋ Add follow-up</button>')+
    `<div class="dashboard-grid"><section class="panel"><div class="panel-header"><div><h3>Follow-up reminders</h3><p>Stay ahead of customer communication</p></div><button class="panel-link">View calendar</button></div><div class="alert-list"><div class="alert"><div class="alert-icon">◷</div><div><b>Call Priya Nair</b><small>Warranty check-in · Today at 2:30 PM</small></div><span class="status status-orange">Due today</span></div><div class="alert"><div class="alert-icon">◷</div><div><b>Email Marcus Reed</b><small>Screen replacement ready · Tomorrow</small></div><span class="status status-blue">Upcoming</span></div><div class="alert"><div class="alert-icon">◷</div><div><b>Follow up with Jordan Lee</b><small>Collect feedback · Sep 12</small></div><span class="status status-gray">Upcoming</span></div></div></section><section class="panel"><div class="panel-header"><div><h3>Customer health</h3><p>Based on recent activity and history</p></div></div><div class="metric-grid" style="margin:0;grid-template-columns:1fr 1fr"><div class="metric" style="padding:8px"><label>Returning customers</label><strong>248</strong><small>19.3% of total</small></div><div class="metric" style="padding:8px"><label>New this month</label><strong>86</strong><small>↗ 12.4%</small></div></div></section></div><section class="panel recent"><div class="panel-header"><div><h3>Recent customer activity</h3><p>Notes and interactions from your team</p></div><button class="panel-link">View all activity →</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Customer</th><th>Activity</th><th>By</th><th>Date</th><th></th></tr></thead><tbody><tr><td>${person('Priya Nair')}</td><td>Added note: “Prefers text updates”</td><td>Alex Morgan</td><td>Today, 10:42 AM</td><td><button class="panel-link">View</button></td></tr><tr><td>${person('Marcus Reed')}</td><td>Invoice INV-0870 marked as paid</td><td>Jamie Cole</td><td>Today, 9:18 AM</td><td><button class="panel-link">View</button></td></tr><tr><td>${person('Jordan Lee')}</td><td>Repair job JOB-1045 created</td><td>Alex Morgan</td><td>Yesterday</td><td><button class="panel-link">View</button></td></tr></tbody></table></div></section>`;
}

function renderReports() {
  return layout('Reports & analytics','Understand revenue, repair throughput and technician performance.','<button class="button button-ghost">▣ Print report</button>')+
    `<div class="toolbar"><select class="select"><option>September 2024</option><option>August 2024</option><option>July 2024</option></select><select class="select"><option>All locations</option><option>TechServe HQ</option></select><span class="spacer"></span><button class="button button-light">⇩ Export CSV</button></div><div class="dashboard-grid"><section class="panel"><div class="panel-header"><div><h3>Technician performance</h3><p>Completed repairs and average turnaround</p></div></div><table class="data-table"><thead><tr><th>Technician</th><th>Completed</th><th>Avg. time</th><th>Customer rating</th></tr></thead><tbody><tr><td>${person('Noah Williams')}</td><td><b>42</b></td><td>2.4 days</td><td><span style="color:#e99a36">★★★★★</span> 4.9</td></tr><tr><td>${person('Sofia Patel')}</td><td><b>38</b></td><td>2.1 days</td><td><span style="color:#e99a36">★★★★★</span> 4.8</td></tr><tr><td>${person('Liam Chen')}</td><td><b>31</b></td><td>2.8 days</td><td><span style="color:#e99a36">★★★★☆</span> 4.6</td></tr></tbody></table></section><section class="panel"><div class="panel-header"><div><h3>Most repaired device types</h3><p>September repair volume</p></div></div><div class="alert-list"><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>Laptops</b><small>82 repairs</small></div><b>46%</b></div><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>MacBooks</b><small>44 repairs</small></div><b>25%</b></div><div class="alert"><div class="alert-icon">▤</div><div style="flex:1"><b>Desktops</b><small>31 repairs</small></div><b>17%</b></div></div></section></div><section class="panel recent"><div class="panel-header"><div><h3>Report snapshot</h3><p>Key operational metrics for this period</p></div></div><div class="metric-grid"><div class="metric"><label>Revenue</label><strong>$24,680</strong><small>+18.6% month over month</small></div><div class="metric"><label>Repairs completed</label><strong>126</strong><small>+15.2% month over month</small></div><div class="metric"><label>Avg. ticket value</label><strong>$195.87</strong><small>+4.8% month over month</small></div></div></section>`;
}

function renderSettings() {
  return layout('Settings','Manage your workspace preferences and staff access.','<button class="button button-primary">Save changes</button>')+
    `<section class="panel"><div class="panel-header"><div><h3>Workspace settings</h3><p>These settings apply to your TechServe workspace.</p></div></div><div class="form-grid" style="max-width:700px"><label>Business name<input value="TechServe HQ"></label><label>Timezone<select><option>Pacific Time (UTC-08:00)</option></select></label><label>Business email<input value="hello@techserve.example"></label><label>Default currency<select><option>USD — US Dollar</option></select></label></div></section><section class="panel recent"><div class="panel-header"><div><h3>Team members</h3><p>Role-based access for your repair shop</p></div><button class="button button-light">＋ Invite member</button></div><table class="data-table"><thead><tr><th>Member</th><th>Role</th><th>Last active</th><th>Status</th></tr></thead><tbody><tr><td>${person('Alex Morgan','AM')}</td><td>Administrator</td><td>Just now</td><td>${status('Completed')}</td></tr><tr><td>${person('Jamie Cole','JC')}</td><td>Cashier / Staff</td><td>Today, 9:18 AM</td><td>${status('Completed')}</td></tr><tr><td>${person('Noah Williams','NW')}</td><td>Technician</td><td>Today, 8:54 AM</td><td>${status('Completed')}</td></tr></tbody></table></section>`;
}

function render() {
  applyRoleProfile();
  applyNavigationAccess();
  const views = {
    overview: renderOverview,
    repairs: renderRepairs,
    customers: renderCustomers,
    devices: renderDevices,
    inventory: renderInventory,
    billing: renderBilling,
    crm: renderCrm,
    reports: renderReports,
    settings: renderSettings
  };
  content.innerHTML = views[state.view]();
  document.getElementById('pageCrumb').textContent = state.view.charAt(0).toUpperCase() + state.view.slice(1);
  document.querySelectorAll('.nav-item[data-view]').forEach(item => item.classList.toggle('active', item.dataset.view === state.view));
  bindViewActions();
}

function openModal(){
  document.getElementById('modalBackdrop').classList.add('open');
  const dateInput = document.querySelector('#jobForm input[type="date"]');
  if (dateInput) dateInput.valueAsDate = new Date();
}

function closeModal(){
  document.getElementById('modalBackdrop').classList.remove('open');
}

function bindViewActions(){
  document.querySelectorAll('[data-view-link]').forEach(btn => btn.onclick = () => {
    state.view = btn.dataset.viewLink;
    render();
  });

  const newJob = document.getElementById('newJob');
  if (newJob) newJob.onclick = openModal;

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
}

const defaultAuthUsers = {
  admin: { password: 'admin123', role: 'ADMIN' },
  manager: { password: 'manager123', role: 'MANAGER' },
  technician: { password: 'tech123', role: 'TECHNICIAN' },
  staff: { password: 'staff123', role: 'STAFF' },
  inventory: { password: 'inventory123', role: 'INVENTORY' },
  billing: { password: 'billing123', role: 'BILLING' }
};

function loadAuthUsers() {
  const saved = JSON.parse(localStorage.getItem('techserve_users') || 'null');
  return { ...defaultAuthUsers, ...(saved || {}) };
}

function saveAuthUsers(users) {
  localStorage.setItem('techserve_users', JSON.stringify(users));
}

function syncAuthVisibility() {
  const auth = JSON.parse(localStorage.getItem('techserve_session') || 'null');
  const isLoggedIn = !!auth;
  const loginScreen = document.getElementById('loginScreen');
  const appShell = document.getElementById('appShell');
  if (loginScreen) loginScreen.classList.toggle('hidden', isLoggedIn);
  if (appShell) appShell.classList.toggle('hidden', !isLoggedIn);
  if (isLoggedIn) {
    state.currentUserRole = auth.role;
    state.view = roles[auth.role].defaultView;
    render();
  }
}

function loginUser(username, password) {
  const entry = loadAuthUsers()[(username || '').toLowerCase()];
  const error = document.getElementById('loginError');
  if (!entry || entry.password !== password) {
    if (error) error.classList.remove('hidden');
    return;
  }
  localStorage.setItem('techserve_session', JSON.stringify({ user: username, role: entry.role }));
  state.currentUserRole = entry.role;
  state.view = roles[entry.role].defaultView;
  if (error) error.classList.add('hidden');
  syncAuthVisibility();
}

function registerUser(fullName, username, password, role) {
  const users = loadAuthUsers();
  const key = (username || '').trim().toLowerCase();
  if (!key || users[key]) {
    const error = document.getElementById('registerError');
    if (error) error.classList.remove('hidden');
    return;
  }

  users[key] = { password, role, fullName };
  saveAuthUsers(users);

  const error = document.getElementById('registerError');
  if (error) error.classList.add('hidden');

  localStorage.setItem('techserve_session', JSON.stringify({ user: username, role }));
  state.currentUserRole = role;
  state.view = roles[role].defaultView;
  hideRegisterForm();
  syncAuthVisibility();
}

function logoutUser() {
  localStorage.removeItem('techserve_session');
  syncAuthVisibility();
}

document.querySelectorAll('.nav-item[data-view]').forEach(item => item.onclick = () => {
  state.view = item.dataset.view;
  document.getElementById('sidebar').classList.remove('open');
  render();
});

document.getElementById('menuToggle').onclick = () => document.getElementById('sidebar').classList.toggle('open');

document.getElementById('modalClose').onclick = closeModal;
document.getElementById('modalCancel').onclick = closeModal;
document.getElementById('modalBackdrop').onclick = e => { if (e.target.id === 'modalBackdrop') closeModal(); };

document.getElementById('jobForm').onsubmit = e => {
  e.preventDefault();
  const form = e.target;
  const customer = document.getElementById('jobCustomer').value;
  const device = form.querySelectorAll('select')[1].value;
  const issue = form.querySelector('textarea').value;
  const priority = form.querySelectorAll('select')[2].value;
  state.jobs.unshift({
    id: `JOB-${1050 + state.jobs.length}`,
    customer,
    initials: initials(customer),
    device,
    issue,
    tech: 'Unassigned',
    status: 'Pending',
    priority,
    received: 'Sep 08, 2024',
    due: 'Sep 10, 2024',
    progress: 8
  });
  closeModal();
  state.view = 'repairs';
  render();
};

document.getElementById('globalSearch').oninput = e => {
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

document.getElementById('roleSelector').onchange = e => {
  state.currentUserRole = e.target.value;
  state.view = roles[state.currentUserRole].defaultView;
  render();
};

render();

function showRegisterForm() {
  const registerForm = document.getElementById('registerForm');
  const loginMode = document.getElementById('loginMode');
  if (registerForm) registerForm.classList.remove('hidden');
  if (loginMode) loginMode.classList.add('hidden');
}

function hideRegisterForm() {
  const registerForm = document.getElementById('registerForm');
  const loginMode = document.getElementById('loginMode');
  if (registerForm) registerForm.classList.add('hidden');
  if (loginMode) loginMode.classList.remove('hidden');
}

document.getElementById('loginForm').onsubmit = e => { e.preventDefault(); loginUser(document.getElementById('loginUser').value.trim(), document.getElementById('loginPassword').value); };

document.getElementById('showRegister').onclick = showRegisterForm;
document.getElementById('cancelRegister').onclick = hideRegisterForm;
document.getElementById('registerForm').onsubmit = e => {
  e.preventDefault();
  registerUser(
    document.getElementById('registerName').value.trim(),
    document.getElementById('registerUser').value.trim(),
    document.getElementById('registerPassword').value,
    document.getElementById('registerRole').value
  );
};

document.getElementById('logoutButton').onclick = logoutUser;

syncAuthVisibility();
