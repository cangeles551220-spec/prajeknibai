const roles = {
  ADMIN: {
    initials: 'AD',
    profile: { name: 'Alex Morgan', role: 'Administrator' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'tracking', 'customers', 'devices', 'technicians', 'inventory', 'billing', 'crm', 'reports', 'settings']
  },
  MANAGER: {
    initials: 'MG',
    profile: { name: 'Maya Lopez', role: 'Manager' },
    defaultView: 'overview',
    nav: ['overview', 'repairs', 'tracking', 'customers', 'devices', 'technicians', 'inventory', 'billing', 'crm', 'reports']
  },
  TECHNICIAN: {
    initials: 'TC',
    profile: { name: 'Noah Williams', role: 'Technician' },
    defaultView: 'repairs',
    nav: ['overview', 'repairs', 'tracking', 'devices', 'crm']
  },
  STAFF: {
    initials: 'ST',
    profile: { name: 'Jamie Cole', role: 'Employee / Service Staff' },
    defaultView: 'customers',
    nav: ['overview', 'customers', 'devices', 'repairs', 'tracking', 'billing', 'crm']
  },
  INVENTORY: {
    initials: 'IN',
    profile: { name: 'Riley Chen', role: 'Inventory' },
    defaultView: 'inventory',
    nav: ['overview', 'inventory', 'devices', 'repairs', 'reports']
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
  invoices: [
    {id:'INV-0871',job:'JOB-1045',customer:'Jordan Lee',amount:'$185.00',paid:'$0.00',status:'Unpaid',date:'Sep 08, 2024'},
    {id:'INV-0870',job:'JOB-1044',customer:'Maya Thompson',amount:'$324.50',paid:'$324.50',status:'Paid',date:'Sep 08, 2024'},
    {id:'INV-0869',job:'JOB-1043',customer:'Owen Brooks',amount:'$96.00',paid:'$50.00',status:'Partially Paid',date:'Sep 07, 2024'},
  ],
  assignmentJobId: null
  ,crmCustomerIndex: 0
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
  'Ready for Pickup':'status-green',
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
  const tabs = ['All', 'Pending', 'Diagnosing', 'In Repair', 'Waiting for Parts', 'Ready for Pickup', 'Completed', 'Cancelled'];
  const statusLabel = s => s === 'Testing' ? 'Ready for Pickup' : s;
  const priorityClass = p => p === 'Urgent' || p === 'High' ? 'priority-high' : p === 'Low' ? 'priority-low' : 'priority-medium';
  return `<div class="repair-heading"><div><h1>Repair Jobs</h1><p>${state.jobs.length + 1} total job orders</p></div>${canCreate ? '<button class="button button-primary" id="newJob">＋ Create Repair Job</button>' : ''}</div>
    <div class="repair-tabs">${tabs.map((tab, index) => `<button class="repair-tab ${index === 0 ? 'active' : ''}" data-status-tab="${tab}">${tab}</button>`).join('')}</div>
    <section class="panel repair-search-panel"><input class="table-search" id="tableSearch" placeholder="⌕  Search job ID, customer, technician..."><button class="button button-ghost">▽ Filter</button></section>
    <section class="panel repair-table-panel"><div class="table-wrap"><table class="data-table repair-table" id="repairTable"><thead><tr><th>Job ID</th><th>Customer</th><th>Device</th><th>Problem</th><th>Technician</th><th>Expected</th><th>Priority</th><th>Status</th><th>Cost</th><th>Actions</th></tr></thead><tbody>${state.jobs.map((j, index) => `<tr data-repair-status="${statusLabel(j.status)}"><td><b class="repair-id">RJ-2024-${String(index + 1).padStart(3, '0')}</b></td><td>${j.customer}</td><td>${j.device}</td><td>${j.issue}</td><td>${j.tech}</td><td>${j.due}</td><td><span class="priority-badge ${priorityClass(j.priority)}">${j.priority}</span></td><td>${status(statusLabel(j.status))}</td><td><b>${['₱3,500','₱1,800','₱6,500','₱2,200','₱900'][index] || '₱0'}</b></td><td class="repair-actions"><button>◉</button><button>／</button><button class="assign-technician" data-job-id="${j.id}">♧</button><button>▣</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderTracking() {
  const stages = ['Received', 'Diagnosis', 'In Repair', 'Testing', 'Ready for Pickup', 'Completed'];
  const trackingJobs = [
    { id: 'RJ-2024-001', customer: 'Maria Santos', device: 'Lenovo ThinkPad E15', tech: 'Carlo Mendoza', due: '2024-11-22', received: '2024-11-18', amount: '₱3,500', current: 2 },
    { id: 'RJ-2024-002', customer: 'Jose Reyes', device: 'HP Pavilion TP01', tech: 'Diana Aquino', due: '2024-11-21', received: '2024-11-17', amount: '₱1,800', current: 4 },
    { id: 'RJ-2024-003', customer: 'Ahn Villanueva', device: 'MacBook Air M2', tech: 'Carlo Mendoza', due: '2024-11-20', received: '2024-11-15', amount: '₱6,500', current: 2 },
    { id: 'RJ-2024-004', customer: 'Liza Fernandez', device: 'Dell Inspiron 3891', tech: 'Ben Torres', due: '2024-11-23', received: '2024-11-19', amount: '₱2,200', current: 1 },
    { id: 'RJ-2024-005', customer: 'Rafael Cruz', device: 'ASUS VivoBook 15', tech: 'Diana Aquino', due: '2024-11-24', received: '2024-11-20', amount: '₱900', current: 0 },
    { id: 'RJ-2024-006', customer: 'Marco Dela Rosa', device: 'Dell XPS 15', tech: 'Ben Torres', due: '2024-11-19', received: '2024-11-16', amount: '₱8,500', current: 5 }
  ];
  return layout('Repair Status Tracking', 'Live progress tracking for all active repair jobs') +
    `<div class="tracking-list">${trackingJobs.map(job => `<article class="tracking-card">
      <div class="tracking-header"><div><b class="tracking-id">${job.id}</b><strong>${job.customer}</strong><p>${job.device} · ${job.tech} · Expected: ${job.due}</p></div><div class="tracking-amount"><small>Received: ${job.received}</small><b>${job.amount}</b></div></div>
      <div class="tracking-timeline">${stages.map((stage, index) => `<div class="tracking-stage ${index < job.current ? 'complete' : index === job.current ? 'current' : ''}"><span>${index < job.current ? '✓' : index === job.current ? '◉' : '○'}</span><small>${stage}</small></div>`).join('')}</div>
    </article>`).join('')}</div>`;
}

function renderCustomers() {
  return layout('Customers','Keep customer profiles, contact details and repair history in one place.','<button class="button button-primary" id="newCustomer">＋ Add customer</button>')+
    `<section class="panel"><div class="toolbar"><input class="table-search" id="tableSearch" placeholder="⌕  Search customers..."><select class="select"><option>All customers</option><option>Returning customers</option><option>Active</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Customer</th><th>Customer ID</th><th>Contact</th><th>Email</th><th>Repair jobs</th><th>Status</th><th></th></tr></thead><tbody>${state.customers.map(c=>`<tr><td>${person(c.name)}</td><td>${c.id}</td><td>${c.contact}</td><td>${c.email}</td><td><b>${c.jobs}</b></td><td>${status(c.status)}</td><td><button class="panel-link">View profile</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderDevices() {
  const devices=[['DEV-0084','Priya Nair','Dell XPS 15','DX15-44A91','Windows 11','Good'],['DEV-0083','Marcus Reed','MacBook Pro 14"','C02ZQ0ABMD6T','macOS Sonoma','Good'],['DEV-0082','Elena Cruz','HP EliteBook 840','5CD3412K8L','Windows 10','Fair'],['DEV-0081','Jordan Lee','Lenovo ThinkPad T14','PF3X9L7M','Windows 11','Good']];
  return layout('Devices','A complete view of every device registered with your shop.','<button class="button button-primary">＋ Register device</button>')+`<section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search device, serial or customer..."><select class="select"><option>All device types</option><option>Laptop</option><option>Desktop</option><option>MacBook</option></select><span class="spacer"></span><button class="button button-ghost">⇩ Export</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Device ID</th><th>Customer</th><th>Device</th><th>Serial number</th><th>Operating system</th><th>Condition</th><th>Repair history</th></tr></thead><tbody>${devices.map(d=>`<tr><td><b>${d[0]}</b></td><td>${person(d[1])}</td><td><b>${d[2]}</b></td><td class="tag">${d[3]}</td><td>${d[4]}</td><td>${status(d[5]==='Fair'?'Diagnosing':'Completed')}</td><td><button class="panel-link">2 jobs →</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderTechnicians() {
  const technicians = [
    { name: 'Carlo Mendoza', specialty: 'Hardware Repair, Motherboard', availability: 'Busy', jobs: 3, completed: 47, capacity: 5 },
    { name: 'Diana Aquino', specialty: 'Software, OS Installation', availability: 'Available', jobs: 2, completed: 63, capacity: 5 },
    { name: 'Ben Torres', specialty: 'Screen Replacement, Apple', availability: 'Busy', jobs: 2, completed: 38, capacity: 5 },
    { name: 'Grace Lim', specialty: 'Networking, Data Recovery', availability: 'Available', jobs: 0, completed: 55, capacity: 5 },
    { name: 'Ryan Castillo', specialty: 'Laptop Repair, Soldering', availability: 'On Leave', jobs: 1, completed: 29, capacity: 5 }
  ];
  return layout('Technicians', 'Manage technicians and job assignments',
    '<div class="technician-actions"><button class="button button-ghost" id="assignTechnicianJob">♧ Assign Technician</button><button class="button button-primary">＋ Add Technician</button></div>') +
    `<div class="technician-grid">${technicians.map(tech => {
      const code = initials(tech.name);
      const workload = Math.round((tech.jobs / tech.capacity) * 100);
      const availabilityClass = tech.availability === 'Available' ? 'status-green' : tech.availability === 'Busy' ? 'status-orange' : 'status-gray';
      return `<article class="technician-card">
        <div class="technician-card-top"><span class="technician-avatar">${code}</span><div><h3>${tech.name}</h3><p>${tech.specialty}</p></div></div>
        <div class="technician-statuses"><span class="status ${availabilityClass}">${tech.availability}</span><span class="status status-blue">Active</span></div>
        <div class="technician-metrics"><div><b>${tech.jobs}</b><small>Current Jobs</small></div><div><b>${tech.completed}</b><small>Completed</small></div></div>
        <div class="technician-workload"><div><span>Workload</span><span>${tech.jobs}/${tech.capacity}</span></div><i><em style="width:${workload}%"></em></i></div>
        <div class="technician-card-actions"><button class="button button-ghost">View Profile</button><button class="button button-primary assign-technician-card" data-technician="${tech.name}">Assign Job</button></div>
      </article>`;
    }).join('')}</div>`;
}

function renderInventory() {
  const parts = [
    { id: 'P-001', name: 'DDR4 8GB RAM 3200MHz', category: 'Memory', brand: 'Kingston', compatible: 'Laptops/Desktops', stock: 24, reorder: 5, cost: '₱1,450', supplier: 'TechParts PH' },
    { id: 'P-002', name: '2.5" 500GB SSD SATA', category: 'Storage', brand: 'Crucial', compatible: 'Laptops', stock: 3, reorder: 5, cost: '₱2,100', supplier: 'PC Hub Caloocan' },
    { id: 'P-003', name: '15.6" FHD LCD Panel', category: 'Display', brand: 'BOE', compatible: 'Lenovo/Dell 15"', stock: 0, reorder: 2, cost: '₱3,800', supplier: 'ScreenZone PH' },
    { id: 'P-004', name: '60W USB-C Charger Brick', category: 'Power', brand: 'Belkin', compatible: 'Universal', stock: 18, reorder: 4, cost: '₱850', supplier: 'TechParts PH' },
    { id: 'P-005', name: 'Laptop Cooling Fan', category: 'Cooling', brand: 'Generic', compatible: 'ASUS/HP', stock: 7, reorder: 3, cost: '₱380', supplier: 'PC Hub Caloocan' },
    { id: 'P-006', name: 'MacBook Keyboard (M1)', category: 'Input', brand: 'Apple', compatible: 'MacBook Air M1', stock: 2, reorder: 2, cost: '₱7,200', supplier: 'Apple PH Parts' },
    { id: 'P-007', name: '500W ATX PSU 80+ Bronze', category: 'Power', brand: 'Corsair', compatible: 'Desktops', stock: 11, reorder: 3, cost: '₱3,200', supplier: 'TechParts PH' }
  ];
  const stockStatus = part => part.stock === 0 ? '<span class="inventory-status inventory-out">Out of Stock</span>' : part.stock <= part.reorder ? '<span class="inventory-status inventory-low">Low Stock</span>' : '<span class="inventory-status inventory-in">In Stock</span>';
  return layout('Parts Inventory', 'Manage parts and stock levels',
    '<div class="inventory-actions"><button class="button inventory-stock-in">⇩ Stock In</button><button class="button inventory-stock-out">⇧ Stock Out</button><button class="button button-primary">＋ Add Part</button></div>') +
    `<div class="inventory-summary"><div><b>7</b><span>Total Parts</span></div><div><b class="summary-green">4</b><span>In Stock</span></div><div><b class="summary-orange">2</b><span>Low Stock</span></div><div><b class="summary-red">1</b><span>Out of Stock</span></div><div><b class="summary-teal">₱108.7k</b><span>Inventory Value</span></div></div>
    <section class="panel inventory-panel"><input class="table-search inventory-search" id="inventorySearch" placeholder="⌕  Search by name, ID, or category..."><div class="table-wrap"><table class="data-table inventory-table"><thead><tr><th>Part ID</th><th>Name</th><th>Category</th><th>Brand</th><th>Compatible</th><th>Qty</th><th>Reorder</th><th>Unit cost</th><th>Supplier</th><th>Status</th><th>Actions</th></tr></thead><tbody>${parts.map(p=>`<tr><td><a class="inventory-id">${p.id}</a></td><td><b>${p.name}</b></td><td><span class="inventory-category">${p.category}</span></td><td>${p.brand}</td><td>${p.compatible}</td><td><b class="${p.stock === 0 ? 'qty-out' : p.stock <= p.reorder ? 'qty-low' : 'qty-good'}">${p.stock}</b></td><td>${p.reorder}</td><td><b>${p.cost}</b></td><td>${p.supplier}</td><td>${stockStatus(p)}</td><td><button class="inventory-icon-button">⌕</button><button class="inventory-icon-button delete">▣</button></td></tr>`).join('')}</tbody></table></div></section>`;
}

function renderBilling() {
  return layout('Billing & invoices','Create invoices, record payments and keep balances clear.','<button class="button button-primary">＋ Create invoice</button>')+
    `<div class="metric-grid"><div class="panel metric"><label>Collected this month</label><strong>$18,420</strong><small>↗ 18.6% vs last month</small></div><div class="panel metric"><label>Outstanding balance</label><strong style="color:var(--orange)">$4,875</strong><small style="color:var(--orange)">12 invoices unpaid</small></div><div class="panel metric"><label>Average invoice</label><strong>$194.60</strong><small>Across 126 completed repairs</small></div></div><section class="panel"><div class="toolbar"><input class="table-search" placeholder="⌕  Search invoice or customer..."><select class="select"><option>All payment statuses</option><option>Paid</option><option>Partially Paid</option><option>Unpaid</option></select><span class="spacer"></span><button class="button button-ghost">▣ Print report</button></div><div class="table-wrap"><table class="data-table"><thead><tr><th>Invoice</th><th>Job order</th><th>Customer</th><th>Amount</th><th>Paid</th><th>Balance</th><th>Status</th><th>Date</th></tr></thead><tbody>${state.invoices.map(i=>{ const amount=parseFloat(i.amount.replace(/[$,]/g,'')); const paid=parseFloat(i.paid.replace(/[$,]/g,'')); const balance=amount-paid; return `<tr><td><b>${i.id}</b></td><td>${i.job}</td><td>${person(i.customer)}</td><td><b>${i.amount}</b></td><td>${i.paid}</td><td><b>${money(balance)}</b></td><td>${status(i.status)}</td><td>${i.date}</td></tr>`; }).join('')}</tbody></table></div></section>`;
}

function renderCrm() {
  const customers = [
    { name: 'Maria Santos', id: 'C-001', initials: 'MS', phone: '0917-234-5678', email: 'maria.santos@gmail.com', address: '42 Rizal St, Makati', device: 'Lenovo ThinkPad E15', job: 'RJ-2024-001', status: 'In Repair', cost: '₱3,500', invoice: 'INV-2024-003', date: '2024-11-22', paid: 'Partially Paid', repairs: 7, spent: '₱3,500', joined: 'Nov 18', notes: 3 },
    { name: 'Jose Reyes', id: 'C-002', initials: 'JR', phone: '0918-456-7890', email: 'jose.reyes@gmail.com', address: '18 Mabini St, Quezon City', device: 'HP Pavilion TP01', job: 'RJ-2024-002', status: 'Ready for Pickup', cost: '₱1,800', invoice: 'INV-2024-004', date: '2024-11-21', paid: 'Paid', repairs: 4, spent: '₱8,200', joined: 'Nov 17', notes: 2 },
    { name: 'Ana Villanueva', id: 'C-003', initials: 'AV', phone: '0919-222-3456', email: 'ana.villanueva@gmail.com', address: '9 Jupiter St, Makati', device: 'MacBook Air M2', job: 'RJ-2024-003', status: 'Waiting for Parts', cost: '₱6,500', invoice: 'INV-2024-005', date: '2024-11-20', paid: 'Partially Paid', repairs: 3, spent: '₱12,500', joined: 'Nov 15', notes: 4 },
    { name: 'Marco Dela Rosa', id: 'C-004', initials: 'MD', phone: '0920-333-4567', email: 'marco.delarosa@gmail.com', address: '7 P. Gomez St, Manila', device: 'Dell XPS 15', job: 'RJ-2024-006', status: 'Completed', cost: '₱8,500', invoice: 'INV-2024-006', date: '2024-11-19', paid: 'Paid', repairs: 6, spent: '₱24,000', joined: 'Nov 16', notes: 5 }
  ];
  const customer = customers[state.crmCustomerIndex] || customers[0];
  return `<div class="crm-heading"><div><h1>Customer CRM</h1><p>Customer relationships and service history</p></div><div class="crm-customer-tabs">${customers.map((item, index) => `<button class="${index === state.crmCustomerIndex ? 'active' : ''}" data-crm-customer="${index}">${item.name.split(' ')[0]}</button>`).join('')}</div></div>
    <div class="crm-layout"><aside class="crm-profile-card"><div class="crm-avatar">${customer.initials}</div><h2>${customer.name}</h2><p class="crm-id">${customer.id}</p><span class="crm-active">● Active Customer</span><div class="crm-contact"><p>♧ ${customer.phone}</p><p>✉ ${customer.email}</p><p>⌖ ${customer.address}</p></div><div class="crm-profile-actions"><button class="button button-ghost">Edit</button><button class="button button-primary">New Job</button></div><div class="crm-stats"><div><b>${customer.repairs}</b><small>Total Repairs</small></div><div><b>${customer.spent}</b><small>Total Spent</small></div><div><b>${customer.joined}</b><small>Last Visit</small></div><div><b>${customer.notes}</b><small>Notes</small></div></div></aside>
    <main class="crm-details"><section class="panel crm-section"><h3>Repair History</h3><table class="data-table"><thead><tr><th>JOB ID</th><th>DEVICE</th><th>PROBLEM</th><th>STATUS</th><th>COST</th></tr></thead><tbody><tr><td><b class="repair-id">${customer.job}</b></td><td>${customer.device}</td><td>Laptop not turning on, battery drains immediately</td><td>${status(customer.status)}</td><td><b>${customer.cost}</b></td></tr></tbody></table></section>
    <section class="panel crm-section"><h3>Billing History</h3><table class="data-table"><thead><tr><th>INVOICE</th><th>DATE</th><th>TOTAL</th><th>STATUS</th></tr></thead><tbody><tr><td><b class="repair-id">${customer.invoice}</b></td><td>${customer.date}</td><td><b>${customer.cost}</b></td><td>${status(customer.paid)}</td></tr></tbody></table></section>
    <section class="panel crm-section crm-timeline"><h3>Customer Timeline</h3><div><p>● &nbsp; New repair job created: ${customer.job}<small>Nov 18, 2024</small></p><p>● &nbsp; Payment received: ${customer.cost} for RJ-2024-006<small>Nov 10, 2024</small></p><p>● &nbsp; Device checked in: ${customer.device}<small>Oct 28, 2024</small></p><p>● &nbsp; Customer note updated by Mark Bautista</p></div></section></main></div>`;
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
    tracking: renderTracking,
    customers: renderCustomers,
    devices: renderDevices,
    technicians: renderTechnicians,
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
  if (state.view === 'customers') loadLiveCustomers();
}

async function loadLiveCustomers() {
  const response = await fetch('/api/customers');
  if (!response.ok) throw new Error('Unable to load customers from the database.');
  const records = await response.json();
  state.customers = records.map(customer => ({
    id: `CUS-${String(customer.id).padStart(4, '0')}`,
    name: customer.name,
    contact: customer.contact,
    email: customer.email,
    address: customer.address,
    jobs: customer.jobs,
    status: customer.status
  }));
  if (state.view === 'customers') {
    content.innerHTML = renderCustomers();
    bindViewActions();
  }
}

function openModal(){
  document.getElementById('modalBackdrop').classList.add('open');
  const dateInput = document.querySelector('#jobForm input[type="date"]');
  if (dateInput) dateInput.valueAsDate = new Date();
}

function closeModal(){
  document.getElementById('modalBackdrop').classList.remove('open');
}

function openCustomerModal(){
  document.getElementById('customerModalBackdrop').classList.add('open');
  document.getElementById('customerForm').reset();
}

function closeCustomerModal(){
  document.getElementById('customerModalBackdrop').classList.remove('open');
}

function openAssignmentModal(jobId) {
  const job = state.jobs.find(item => item.id === jobId);
  if (!job) return;
  state.assignmentJobId = jobId;
  document.getElementById('assignmentTechnician').value = job.tech === 'Unassigned' ? '' : job.tech;
  document.getElementById('assignmentModalBackdrop').classList.add('open');
}

function closeAssignmentModal() {
  document.getElementById('assignmentModalBackdrop').classList.remove('open');
  state.assignmentJobId = null;
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

  document.querySelectorAll('.assign-technician').forEach(button => {
    button.onclick = () => openAssignmentModal(button.dataset.jobId);
  });
  document.querySelectorAll('.assign-technician-card').forEach(button => {
    button.onclick = () => {
      state.view = 'repairs';
      render();
    };
  });
  const assignTechnicianJob = document.getElementById('assignTechnicianJob');
  if (assignTechnicianJob) assignTechnicianJob.onclick = () => {
    state.view = 'repairs';
    render();
  };

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
  admin: { password: 'Admin@123', role: 'ADMIN' },
  manager: { password: 'Manager@123', role: 'MANAGER' },
  employee: { password: 'Employee@123', role: 'STAFF' },
  technician: { password: 'Technician@123', role: 'TECHNICIAN' },
  inventory: { password: 'Inventory@123', role: 'INVENTORY' },
  billing: { password: 'Billing@123', role: 'BILLING' }
};

function loadAuthUsers() {
  const saved = JSON.parse(localStorage.getItem('techserve_users') || 'null');
  return { ...(saved || {}), ...defaultAuthUsers };
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
    state.view = roles[auth.role].defaultView;
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

function registerUser(fullName, username, password, role) {
  const users = loadAuthUsers();
  const key = (username || '').trim().toLowerCase();
  const validName = (fullName || '').trim().length >= 2;
  const validUsername = /^[a-z0-9._-]{3,30}$/.test(key);
  const validPassword = typeof password === 'string' && password.length >= 8;
  if (!validName || !validUsername || !validPassword || !roles[role] || users[key]) {
    const error = document.getElementById('registerError');
    if (error) error.textContent = 'Use a valid name, username, role, and password of at least 8 characters.';
    if (error) error.classList.remove('hidden');
    return;
  }

  users[key] = { password, role, fullName };
  saveAuthUsers(users);

  const error = document.getElementById('registerError');
  if (error) error.classList.add('hidden');

  localStorage.setItem('techserve_session', JSON.stringify({
    user: key,
    role,
    createdAt: Date.now()
  }));
  logActivity('account_registered');
  state.currentUserRole = role;
  state.view = roles[role].defaultView;
  hideRegisterForm();
  syncAuthVisibility();
}

function logoutUser() {
  logActivity('logout');
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
document.getElementById('customerModalClose').onclick = closeCustomerModal;
document.getElementById('customerModalCancel').onclick = closeCustomerModal;
document.getElementById('customerModalBackdrop').onclick = e => {
  if (e.target.id === 'customerModalBackdrop') closeCustomerModal();
};
document.getElementById('assignmentModalClose').onclick = closeAssignmentModal;
document.getElementById('assignmentModalCancel').onclick = closeAssignmentModal;
document.getElementById('assignmentModalBackdrop').onclick = e => {
  if (e.target.id === 'assignmentModalBackdrop') closeAssignmentModal();
};

document.getElementById('jobForm').onsubmit = e => {
  e.preventDefault();
  const form = e.target;
  const customer = document.getElementById('jobCustomer').value;
  const device = document.getElementById('jobDevice').value;
  const issue = form.querySelector('textarea').value;
  const priority = document.getElementById('jobPriority').value;
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

document.getElementById('customerForm').onsubmit = async e => {
  e.preventDefault();
  const name = document.getElementById('customerName').value.trim();
  const contact = document.getElementById('customerContact').value.trim();
  const email = document.getElementById('customerEmail').value.trim();
  const address = document.getElementById('customerAddress').value.trim();
  if (name.length < 2 || contact.length < 7 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    document.getElementById('customerEmail').setCustomValidity('Enter a valid customer name, contact number, and email.');
    document.getElementById('customerEmail').reportValidity();
    return;
  }
  document.getElementById('customerEmail').setCustomValidity('');
  const response = await fetch('/api/customers', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, contact, email, address })
  });
  if (!response.ok) {
    document.getElementById('customerEmail').setCustomValidity('The customer could not be saved.');
    document.getElementById('customerEmail').reportValidity();
    return;
  }
  logActivity('customer_created', name);
  closeCustomerModal();
  state.view = 'customers';
  render();
};

document.getElementById('assignmentForm').onsubmit = e => {
  e.preventDefault();
  const job = state.jobs.find(item => item.id === state.assignmentJobId);
  const technician = document.getElementById('assignmentTechnician').value;
  if (!job || !technician) return;
  job.tech = technician;
  logActivity('technician_assigned', `${job.id}: ${job.tech}`);
  closeAssignmentModal();
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
