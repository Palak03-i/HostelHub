/* Progressive enhancement: standard links, forms and full tables work without JS. */
(() => {
  'use strict';
  function init() {
    document.querySelectorAll('[data-enhance-table]').forEach((table, tableIndex) => {
      const body = table.tBodies[0];
      if (!body || !table.tHead) return;
      const rows = Array.from(body.rows).filter(row => !row.querySelector('[colspan]'));
      if (!rows.length) return;
      const host = table.closest('.table-responsive') || table;
      const tools = document.createElement('div');
      tools.className = 'hub-table-tools';
      const label = document.createElement('label');
      label.textContent = 'Search this table';
      const search = document.createElement('input');
      search.type = 'search'; search.className = 'form-control';
      search.id = `hub-search-${tableIndex}`; label.htmlFor = search.id;
      search.placeholder = 'Type to filter records…';
      const clear = document.createElement('button');
      clear.type = 'button'; clear.className = 'btn btn-outline-secondary btn-sm'; clear.textContent = 'Clear';
      tools.append(label, search, clear); host.before(tools);
      const footer = document.createElement('div'); footer.className = 'hub-table-footer';
      const count = document.createElement('span'); count.setAttribute('role','status'); count.setAttribute('aria-live','polite');
      const pages = document.createElement('div'); pages.className = 'hub-table-pages';
      const previous = document.createElement('button'), next = document.createElement('button'), position = document.createElement('span');
      [previous,next].forEach(button => { button.type='button'; button.className='btn btn-outline-secondary btn-sm'; });
      previous.textContent='Previous'; next.textContent='Next'; pages.append(previous,position,next); footer.append(count,pages); host.after(footer);
      const empty = document.createElement('tr'), cell = document.createElement('td');
      cell.colSpan=table.tHead.rows[0].cells.length; cell.className='hub-empty'; cell.textContent='No records match your search. Clear the search to see all records.';
      empty.append(cell); body.append(empty);
      let page=0, ordered=rows.slice(); const pageSize=10;
      const render=() => {
        const term=search.value.trim().toLocaleLowerCase();
        const filtered=ordered.filter(row => row.textContent.toLocaleLowerCase().includes(term));
        const totalPages=Math.max(1,Math.ceil(filtered.length/pageSize)); page=Math.min(page,totalPages-1);
        rows.forEach(row=>row.hidden=true);
        filtered.slice(page*pageSize,(page+1)*pageSize).forEach(row=>row.hidden=false);
        empty.hidden=filtered.length!==0;
        count.textContent=`${filtered.length ? page*pageSize+1 : 0}–${Math.min((page+1)*pageSize,filtered.length)} of ${filtered.length} records`;
        position.textContent=`Page ${page+1} of ${totalPages}`;
        previous.disabled=page===0; next.disabled=page>=totalPages-1;
      };
      search.addEventListener('input',()=>{page=0;render();});
      clear.addEventListener('click',()=>{search.value='';page=0;render();search.focus();});
      previous.addEventListener('click',()=>{page--;render();}); next.addEventListener('click',()=>{page++;render();});
      Array.from(table.tHead.rows[0].cells).forEach((heading,index)=>{
        if (/actions?/i.test(heading.textContent) || !heading.textContent.trim()) return;
        const text=heading.textContent.trim(), button=document.createElement('button');
        button.type='button'; button.className='hub-sort'; button.textContent=text+' ↕';
        button.setAttribute('aria-label',`Sort by ${text}`); heading.replaceChildren(button);
        let ascending=false;
        button.addEventListener('click',()=>{
          ascending=!ascending;
          table.querySelectorAll('th[aria-sort]').forEach(th=>th.removeAttribute('aria-sort'));
          heading.setAttribute('aria-sort',ascending?'ascending':'descending');
          ordered.sort((a,b)=>(ascending?1:-1)*a.cells[index].textContent.trim().localeCompare(b.cells[index].textContent.trim(),undefined,{numeric:true,sensitivity:'base'}));
          ordered.forEach(row=>body.append(row)); body.append(empty); page=0; render();
        });
      });
      // Printing should include all filtered records, not just the current page.
      window.addEventListener('beforeprint',()=>{ const term=search.value.trim().toLocaleLowerCase(); rows.forEach(row=>row.hidden=!row.textContent.toLocaleLowerCase().includes(term)); });
      window.addEventListener('afterprint',render); render();
    });
    document.querySelectorAll('textarea').forEach((input,index)=>{
      const max=input.getAttribute('maxlength') || input.dataset.valLengthMax;
      if (!max) return;
      input.maxLength=Number(max);
      const count=document.createElement('small'); count.className='hub-char-count'; count.id=`hub-count-${index}`;
      input.setAttribute('aria-describedby',`${input.getAttribute('aria-describedby') || ''} ${count.id}`.trim()); input.after(count);
      const update=()=>{count.textContent=`${input.value.length} / ${max} characters`;}; input.addEventListener('input',update); update();
    });
    // Run after page and jQuery validation handlers. Keep submitter values in the POST.
    document.addEventListener('submit', event => {
      const form=event.target;
      if (!(form instanceof HTMLFormElement) || form.method.toLowerCase()!=='post' || event.defaultPrevented) return;
      if (form.dataset.submitting==='true') { event.preventDefault(); return; }
      if (!form.checkValidity() || (window.jQuery && window.jQuery.fn.valid && !window.jQuery(form).valid())) return;
      form.dataset.submitting='true'; form.setAttribute('aria-busy','true');
      const status=document.createElement('div'); status.className='hub-submit-status'; status.setAttribute('role','status'); status.textContent='Saving, please wait…'; form.append(status);
    });
    window.addEventListener('pageshow',()=>document.querySelectorAll('form[aria-busy]').forEach(form=>{delete form.dataset.submitting;form.removeAttribute('aria-busy');form.querySelector('.hub-submit-status')?.remove();}));
    document.querySelectorAll('.alert-danger,.validation-summary-errors').forEach(el=>el.setAttribute('role','alert'));
    document.querySelectorAll('.alert-success').forEach(el=>el.setAttribute('role','status'));
  }
  if (document.readyState==='loading') document.addEventListener('DOMContentLoaded',init); else init();
})();
