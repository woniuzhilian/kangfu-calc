'use strict';
// 单建地下室整体抗浮计算 —— 纯逻辑，逐行移植自 Jisuan.cs 的 KangfuJisuan.Jisuan。
// C# 里输入参数声明为 float(32位)，乘除中间量按 float 截断；这里用 Math.fround 逐步复现，
// 以保证与桌面版得到完全相同的比值/计算书。浏览器与 Node 测试共用本文件。

const GW = 10; // GammaWater，水的重度

// 字段定义：id=桌面版控件名, key=参数名, name=校验报错用名称
const OVERALL = [
  {id:'shuiwei', key:'sw', name:'抗浮设防水位标高'},
  {id:'dibandibiaogao', key:'jcd', name:'基础底板底标高'},
  {id:'tongrongzhong', key:'trz', name:'混凝土容重'},
  {id:'dibanfuturongzhong', key:'dftrz', name:'底板覆土容重'},
  {id:'dibanfutuhou', key:'dfth', name:'底板覆土厚'},
];
const FOUND = [
  {id:'jichuhou', key:'jch', name:'基础厚度(含底板厚)'},
  {id:'jichuchang', key:'jcc', name:'基础长度'},
  {id:'jichukuan', key:'jck', name:'基础宽度'},
  {id:'jichushuliang', key:'jcs', name:'基础数量'},
  {id:'dibanhoudu', key:'jcbh', name:'基础底板厚度'},
];
const F1 = [
  {id:'yidingbanbiaogao', key:'ydbg', name:'-1层顶板顶标高'},
  {id:'fuyidingbanfutuhou', key:'yfth', name:'-1层顶板覆土厚'},
  {id:'fuyidingbanfuturongzhong', key:'yftrz', name:'-1层顶板覆土容重'},
  {id:'fuyidingbanhou', key:'ydbh', name:'-1层顶板厚'},
  {id:'fuyixzhuliangkuan', key:'yxzlk', name:'-1层x向主梁宽'},
  {id:'fuyixzhulianggao', key:'yxzlg', name:'-1层x向主梁高'},
  {id:'fuyixzhuliangshu', key:'yxzls', name:'-1层x向主梁数量'},
  {id:'fuyiyzhuliangkuan', key:'yyzlk', name:'-1层y向主梁宽'},
  {id:'fuyiyzhulianggao', key:'yyzlg', name:'-1层y向主梁高'},
  {id:'fuyiyzhuliangshu', key:'yyzls', name:'-1层y向主梁数量'},
  {id:'fuyixciliangkuan', key:'yxclk', name:'-1层x向次梁宽'},
  {id:'fuyixcilianggao', key:'yxclg', name:'-1层x向次梁高'},
  {id:'fuyixciliangshu', key:'yxcls', name:'-1层x向次梁数量'},
  {id:'fuyiyciliangkuan', key:'yyclk', name:'-1层y向次梁宽'},
  {id:'fuyiycilianggao', key:'yyclg', name:'-1层y向次梁高'},
  {id:'fuyiyciliangshu', key:'yycls', name:'-1层y向次梁数量'},
  {id:'fuyizhuchang', key:'yzc', name:'-1层柱长度'},
  {id:'fuyizhukuan', key:'yzk', name:'-1层柱宽度'},
  {id:'fuyizhushu', key:'yzs', name:'-1层柱数量'},
];
const F2 = [
  {id:'erdingbanbiaogao', key:'edbg', name:'-2层顶板顶标高'},
  {id:'fuerdingbanfutuhou', key:'efth', name:'-2层顶板覆土厚'},
  {id:'fuerdingbanfuturongzhong', key:'eftrz', name:'-2层顶板覆土容重'},
  {id:'fuerdingbanhou', key:'edbh', name:'-2层顶板厚'},
  {id:'fuerxzhuliangkuan', key:'exzlk', name:'-2层x向主梁宽'},
  {id:'fuerxzhulianggao', key:'exzlg', name:'-2层x向主梁高'},
  {id:'fuerxzhuliangshu', key:'exzls', name:'-2层x向主梁数量'},
  {id:'fueryzhuliangkuan', key:'eyzlk', name:'-2层y向主梁宽'},
  {id:'fueryzhulianggao', key:'eyzlg', name:'-2层y向主梁高'},
  {id:'fueryzhuliangshu', key:'eyzls', name:'-2层y向主梁数量'},
  {id:'fuerxciliangkuan', key:'exclk', name:'-2层x向次梁宽'},
  {id:'fuerxcilianggao', key:'exclg', name:'-2层x向次梁高'},
  {id:'fuerxciliangshu', key:'excls', name:'-2层x向次梁数量'},
  {id:'fueryciliangkuan', key:'eyclk', name:'-2层y向次梁宽'},
  {id:'fuerycilianggao', key:'eyclg', name:'-2层y向次梁高'},
  {id:'fueryciliangshu', key:'eycls', name:'-2层y向次梁数量'},
  {id:'fuerzhuchang', key:'ezc', name:'-2层柱长度'},
  {id:'fuerzhukuan', key:'ezk', name:'-2层柱宽度'},
  {id:'fuerzhushu', key:'ezs', name:'-2层柱数量'},
];
const F3 = [
  {id:'sandingbanbiaogao', key:'sdbg', name:'-3层顶板顶标高'},
  {id:'fusandingbanfutuhou', key:'sfth', name:'-3层顶板覆土厚'},
  {id:'fusandingbanfuturongzhong', key:'sftrz', name:'-3层顶板覆土容重'},
  {id:'fusandingbanhou', key:'sdbh', name:'-3层顶板厚'},
  {id:'fusanxzhuliangkuan', key:'sxzlk', name:'-3层x向主梁宽'},
  {id:'fusanxzhulianggao', key:'sxzlg', name:'-3层x向主梁高'},
  {id:'fusanxzhuliangshu', key:'sxzls', name:'-3层x向主梁数量'},
  {id:'fusanyzhuliangkuan', key:'syzlk', name:'-3层y向主梁宽'},
  {id:'fusanyzhulianggao', key:'syzlg', name:'-3层y向主梁高'},
  {id:'fusanyzhuliangshu', key:'syzls', name:'-3层y向主梁数量'},
  {id:'fusanxciliangkuan', key:'sxclk', name:'-3层x向次梁宽'},
  {id:'fusanxcilianggao', key:'sxclg', name:'-3层x向次梁高'},
  {id:'fusanxciliangshu', key:'sxcls', name:'-3层x向次梁数量'},
  {id:'fusanyciliangkuan', key:'syclk', name:'-3层y向次梁宽'},
  {id:'fusanycilianggao', key:'syclg', name:'-3层y向次梁高'},
  {id:'fusanyciliangshu', key:'sycls', name:'-3层y向次梁数量'},
  {id:'fusanzhuchang', key:'szc', name:'-3层柱长度'},
  {id:'fusanzhukuan', key:'szk', name:'-3层柱宽度'},
  {id:'fusanzhushu', key:'szs', name:'-3层柱数量'},
];

// 模拟 C# 的 ToString("F2"/"F4")：按 double 的精确二进制值四舍六入五成双（银行家舍入），
// 与 JS toFixed 的"五入"在恰 halfway 值处不同（如 1630.125 → .NET 得 1630.12，toFixed 得 1630.13）。
function fmtFixed(x, d){
  if(!isFinite(x)) return String(x);
  const neg = Math.sign(x)<0 || Object.is(x,-0);
  const ax = Math.abs(x);
  const buf = new DataView(new ArrayBuffer(8)); buf.setFloat64(0, ax);
  const bits = buf.getBigUint64(0);
  const expBi = (bits>>52n)&0x7FFn, man = bits&((1n<<52n)-1n);
  let e, m;
  if(expBi===0n){ e=-1074; m=man; } else { e=Number(expBi)-1075; m=man|(1n<<52n); }  // ax = m*2^e
  const scale = 10n**BigInt(d);
  const num = m*scale;                          // ax*10^d = num*2^e
  let N;
  if(e>=0) N = num<<BigInt(e);
  else {
    const div = 1n<<BigInt(-e);
    const q = num/div, r = num%div, half = div>>1n;
    N = (r>half || (r===half && (q&1n)===1n)) ? q+1n : q;   // round-half-even
  }
  const s = N.toString().padStart(d+1,'0');
  const out = d>0 ? s.slice(0,s.length-d)+'.'+s.slice(s.length-d) : s;
  return (neg?'-':'')+out;
}

// 模拟 float.TryParse(Float, InvariantCulture)：非法返回 null，合法返回 float32 值
function parseFloatCs(s){
  if(typeof s!=='string') return null;
  const t=s.trim();
  if(!/^[+-]?(\d+\.?\d*|\.\d+)$/.test(t)) return null;
  return Math.fround(parseFloat(t));
}

// p：数值参数（均为 float32 值）；R：计算书回显文本（规范化数值字符串）。
function jisuan(p, R){
  const f = Math.fround;
  const dj = p.dj, cs = p.cs;
  const sw = p.sw, jcd = p.jcd, trz = p.trz;

  const mj = f(p.qyx * p.qyy);
  let yzdg=0, ezdg=0, xs, kfxs, zzkfxs;
  if(cs==='1层') yzdg = jcd + p.jcbh;
  else if(cs==='2层'){ yzdg = p.edbg; ezdg = jcd + p.jcbh; }
  else if(cs==='3层'){ yzdg = p.edbg; ezdg = p.sdbg; }

  if(dj==='甲级'){ xs=1.1; kfxs=0.9; zzkfxs=1.0; }
  else if(dj==='乙级'){ xs=1.05; kfxs=0.9; zzkfxs=1.0; }
  else if(dj==='丙级'){ xs=1.05; kfxs=0.95; zzkfxs=1.05; }
  else return { ok:false };

  const yftg = f(f(p.yfth*p.yftrz)*mj) * kfxs;
  const eftg = f(f(p.efth*p.eftrz)*mj) * kfxs;
  const sftg = f(f(p.sfth*p.sftrz)*mj) * kfxs;
  const ybg  = f(f(p.ydbh*trz)*mj) * zzkfxs;
  const ebg  = f(f(p.edbh*trz)*mj) * zzkfxs;
  const sbg  = f(f(p.sdbh*trz)*mj) * zzkfxs;

  function liang(kx,hx,sx,ky,hy,sy,slab){
    const tx = f(f(f(f(kx*f(hx-slab))*p.qyx)*sx)*trz)*zzkfxs;
    const ty = f(f(f(f(ky*f(hy-slab))*p.qyy)*sy)*trz)*zzkfxs;
    return tx+ty;
  }
  const yzlg = liang(p.yxzlk,p.yxzlg,p.yxzls,p.yyzlk,p.yyzlg,p.yyzls,p.ydbh);
  const ezlg = liang(p.exzlk,p.exzlg,p.exzls,p.eyzlk,p.eyzlg,p.eyzls,p.edbh);
  const szlg = liang(p.sxzlk,p.sxzlg,p.sxzls,p.syzlk,p.syzlg,p.syzls,p.sdbh);
  const yclg = liang(p.yxclk,p.yxclg,p.yxcls,p.yyclk,p.yyclg,p.yycls,p.ydbh);
  const eclg = liang(p.exclk,p.exclg,p.excls,p.eyclk,p.eyclg,p.eycls,p.edbh);
  const sclg = liang(p.sxclk,p.sxclg,p.sxcls,p.syclk,p.syclg,p.sycls,p.sdbh);

  const hAvgY = f(f(p.yxzlg+p.yyzlg)/2);
  const hAvgE = f(f(p.exzlg+p.eyzlg)/2);
  const hAvgS = f(f(p.sxzlg+p.syzlg)/2);
  const yzg = f(f(f(p.yzc*p.yzk)*p.yzs)*trz) * (p.ydbg - yzdg - hAvgY) * zzkfxs;
  const ezg = f(f(f(p.ezc*p.ezk)*p.ezs)*trz) * (p.edbg - ezdg - hAvgE) * zzkfxs;
  const szg = f(f(f(p.szc*p.szk)*p.szs)*trz) * (p.sdbg - jcd - p.jcbh - hAvgS) * zzkfxs;

  const jcg  = f(f(f(f(p.jcc*p.jck)*f(p.jch-p.jcbh))*f(trz-GW))*p.jcs) * zzkfxs;
  const jcbg = f(f(p.jcbh*trz)*mj) * zzkfxs;
  const dftg = f(f(p.dfth*p.dftrz)*mj) * kfxs * kfxs;

  let yg = yftg+ybg+yzlg+yclg+yzg;
  let eg = eftg+ebg+ezlg+eclg+ezg;
  let sg = sftg+sbg+szlg+sclg+szg;
  if(cs==='1层'){ eg=0; sg=0; } else if(cs==='2层'){ sg=0; }

  const sfl = (((sw - jcd) * GW) * p.qyx) * p.qyy;
  const sflps = sfl/mj;
  const zizhong = yg+eg+sg+jcg+jcbg+dftg;
  const zzps = zizhong/mj;
  const bz = zzps/sflps;

  let jielun, fuhao, zongjie;
  if(bz>=xs){ jielun='满足抗浮要求！'; fuhao='>'; zongjie='整体抗浮验算满足规范要求。\r\n因整个地下室是该典型的区格形式构成，故只要该区格局部抗浮验算满足，则地下室在考虑底板外挑和墙体重量后的整体抗浮也能满足抗浮要求。'; }
  else { jielun='抗浮不满足要求！'; fuhao='<'; zongjie='整体抗浮验算不满足规范要求！'; }

  let bizhi = fmtFixed(bz,4);
  const e = k=>R[k];
  const zongjie1 =
    '-1层顶板覆土产生的抗浮力标准值='+e('yfth')+'*'+e('yftrz')+'*'+e('qyx')+'*'+e('qyy')+'*'+kfxs+'='+fmtFixed(yftg,2)+'kN\r\n'+
    '-1层顶板自重产生的抗浮力标准值='+e('ydbh')+'*'+e('trz')+'*'+e('qyx')+'*'+e('qyy')+'*'+zzkfxs+'='+fmtFixed(ybg,2)+'kN\r\n'+
    '-1层顶板主梁产生的抗浮力标准值='+e('yxzlk')+'*('+e('yxzlg')+'-'+e('ydbh')+')*'+e('qyx')+'*'+e('yxzls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('yyzlk')+'*('+e('yyzlg')+'-'+e('ydbh')+')*'+e('qyy')+'*'+e('yyzls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(yzlg,2)+'kN\r\n'+
    '-1层顶板次梁产生的抗浮力标准值='+e('yxclk')+'*('+e('yxclg')+'-'+e('ydbh')+')*'+e('qyx')+'*'+e('yxcls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('yyclk')+'*('+e('yyclg')+'-'+e('ydbh')+')*'+e('qyy')+'*'+e('yycls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(yclg,2)+'kN\r\n'+
    '-1层柱子产生的抗浮力标准值='+e('yzc')+'*'+e('yzk')+'*'+e('yzs')+'*'+e('trz')+'*{('+e('ydbg')+')-('+fmtFixed(yzdg,2)+')-[('+e('yxzlg')+'+'+e('yyzlg')+')/2]}*'+zzkfxs+'= '+fmtFixed(yzg,2)+'kN\r\n';
  const zongjie2 =
    '-2层顶板覆土产生的抗浮力标准值='+e('efth')+'*'+e('eftrz')+'*'+e('qyx')+'*'+e('qyy')+'*'+kfxs+'='+fmtFixed(eftg,2)+'kN\r\n'+
    '-2层顶板自重产生的抗浮力标准值='+e('edbh')+'*'+e('trz')+'*'+e('qyx')+'*'+e('qyy')+'*'+zzkfxs+'='+fmtFixed(ebg,2)+'kN\r\n'+
    '-2层顶板主梁产生的抗浮力标准值='+e('exzlk')+'*('+e('exzlg')+'-'+e('edbh')+')*'+e('qyx')+'*'+e('exzls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('eyzlk')+'*('+e('eyzlg')+'-'+e('edbh')+')*'+e('qyy')+'*'+e('eyzls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(ezlg,2)+'kN\r\n'+
    '-2层顶板次梁产生的抗浮力标准值='+e('exclk')+'*('+e('exclg')+'-'+e('edbh')+')*'+e('qyx')+'*'+e('excls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('eyclk')+'*('+e('eyclg')+'-'+e('edbh')+')*'+e('qyy')+'*'+e('eycls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(eclg,2)+'kN\r\n'+
    '-2层柱子产生的抗浮力标准值='+e('ezc')+'*'+e('ezk')+'*'+e('ezs')+'*'+e('trz')+'*{('+e('edbg')+')-('+fmtFixed(ezdg,2)+')-[('+e('exzlg')+'+'+e('eyzlg')+')/2]}*'+zzkfxs+'= '+fmtFixed(ezg,2)+'kN\r\n';
  const zongjie3 =
    '-3层顶板覆土产生的抗浮力标准值='+e('sfth')+'*'+e('sftrz')+'*'+e('qyx')+'*'+e('qyy')+'*'+kfxs+'='+fmtFixed(sftg,2)+'kN\r\n'+
    '-3层顶板自重产生的抗浮力标准值='+e('sdbh')+'*'+e('trz')+'*'+e('qyx')+'*'+e('qyy')+'*'+zzkfxs+'='+fmtFixed(sbg,2)+'kN\r\n'+
    '-3层顶板主梁产生的抗浮力标准值='+e('sxzlk')+'*('+e('sxzlg')+'-'+e('sdbh')+')*'+e('qyx')+'*'+e('sxzls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('syzlk')+'*('+e('syzlg')+'-'+e('sdbh')+')*'+e('qyy')+'*'+e('syzls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(szlg,2)+'kN\r\n'+
    '-3层顶板次梁产生的抗浮力标准值='+e('sxclk')+'*('+e('sxclg')+'-'+e('sdbh')+')*'+e('qyx')+'*'+e('sxcls')+'*'+e('trz')+'*'+zzkfxs+'+'+e('syclk')+'*('+e('syclg')+'-'+e('sdbh')+')*'+e('qyy')+'*'+e('sycls')+'*'+e('trz')+'*'+zzkfxs+'= '+fmtFixed(sclg,2)+'kN\r\n'+
    '-3层柱子产生的抗浮力标准值='+e('szc')+'*'+e('szk')+'*'+e('szs')+'*'+e('trz')+'*{('+e('sdbg')+')-('+e('jcd')+')-'+e('jcbh')+'-[('+e('sxzlg')+'+'+e('syzlg')+')/2]}*'+zzkfxs+'= '+fmtFixed(szg,2)+'kN\r\n';
  let Z2=zongjie2, Z3=zongjie3;
  if(cs==='1层'){ Z2=''; Z3=''; } else if(cs==='2层'){ Z3=''; }

  let zongzizhong;
  if(cs==='1层') zongzizhong='结构自重产生的总抗浮力标准值='+fmtFixed(yg,2)+'+'+fmtFixed(jcg,2)+'+'+fmtFixed(jcbg,2)+'+'+fmtFixed(dftg,2)+'='+fmtFixed(zizhong,2)+'kN';
  else if(cs==='2层') zongzizhong='结构自重产生的总抗浮力标准值='+fmtFixed(yg,2)+'+'+fmtFixed(eg,2)+'+'+fmtFixed(jcg,2)+'+'+fmtFixed(jcbg,2)+'+'+fmtFixed(dftg,2)+'='+fmtFixed(zizhong,2)+'kN';
  else zongzizhong='结构自重产生的总抗浮力标准值='+fmtFixed(yg,2)+'+'+fmtFixed(eg,2)+'+'+fmtFixed(sg,2)+'+'+fmtFixed(jcg,2)+'+'+fmtFixed(jcbg,2)+'+'+fmtFixed(dftg,2)+'='+fmtFixed(zizhong,2)+'kN';

  let jss =
    '************************单建地下室整体抗浮计算书************************\r\n'+
    '一、工程概况\r\n'+
    '工程名称：'+p.mc+'\r\n'+
    '-1层顶板顶标高：'+R.dbgS+'m\r\n'+
    '抗浮设防水位标高：'+R.swS+'m\r\n'+
    '抗浮工程设计等级：'+dj+'\r\n'+
    '地下室层数：'+cs+'\r\n'+
    '基础底板底标高：'+R.jcdS+'m\r\n'+
    '-1层顶板覆土厚度：'+R.yfthS+'m\r\n'+
    '抗浮力组合系数（结构自重）：'+fmtFixed(zzkfxs,2)+'\r\n'+
    '抗浮力组合系数（填筑体）：'+kfxs+'\r\n'+
    '抗浮稳定安全系数最小值：'+xs+'\r\n'+
    '二、规范依据\r\n'+
    '《工程结构通用规范》GB55001-2021\r\n'+
    '《建筑与市政地基基础通用规范》GB55003-2021\r\n'+
    '《建筑结构可靠性设计统一标准》GB50068-2018\r\n'+
    '《建筑工程抗浮技术标准》JGJ475-2019\r\n'+
    '《建筑地基基础设计规范》GB50007-2011\r\n'+
    '《建筑结构荷载规范》GB50009-2012\r\n'+
    '三、抗浮验算\r\n'+
    '取'+R.quyuS+'所围成的区域作为典型区格进行整体抗浮验算，该区域长度为：'+R.qyxS+'m，宽度为：'+R.qyyS+'m\r\n'+
    zongjie1+Z2+Z3+
    '基础底板覆土自重产生的抗浮力标准值='+e('dfth')+'*'+e('dftrz')+'*'+e('qyx')+'*'+e('qyy')+'*'+kfxs+'*'+kfxs+'='+fmtFixed(dftg,2)+'kN\r\n'+
    '基础底板自重产生的抗浮力标准值='+e('jcbh')+'*'+e('trz')+'*'+e('qyx')+'*'+e('qyy')+'*'+zzkfxs+'='+fmtFixed(jcbg,2)+'kN\r\n'+
    '基础自重产生的抗浮力标准值='+e('jcc')+'*'+e('jck')+'*('+e('jch')+'-'+e('jcbh')+')*('+e('trz')+'-'+GW+')*'+e('jcs')+'*'+zzkfxs+'='+fmtFixed(jcg,2)+'kN\r\n'+
    zongzizhong+'\r\n'+
    '结构构件自重产生的单位面积抗浮力标准值='+fmtFixed(zizhong,2)+'/'+e('qyx')+'/'+e('qyy')+'='+fmtFixed(zzps,2)+'kN\r\n'+
    '水浮力总作用值=[('+fmtFixed(sw,2)+')-('+fmtFixed(jcd,2)+')]*'+GW+'*'+e('qyx')+'*'+e('qyy')+'='+fmtFixed(sfl,2)+'kN\r\n'+
    '单位面积水浮力作用值='+fmtFixed(sfl,2)+'/'+e('qyx')+'/'+e('qyy')+'='+fmtFixed(sflps,2)+'kN\r\n'+
    '抗浮稳定安全系数='+fmtFixed(zzps,2)+'/'+fmtFixed(sflps,2)+'='+fmtFixed(bz,4)+'\r\n'+
    '四、结论\r\n'+
    '根据《建筑地基基础设计规范》（GB50007-2011）第5.4.3条规定，'+fmtFixed(bz,4)+fuhao+xs+'。'+zongjie;

  if(jcd >= sw){ jss='抗浮水位低于基础底板底，无需进行抗浮验算！'; jielun=''; bizhi=''; }
  return { ok:true, bizhi, jielun, jss };
}

// 从"原始文本映射"跑完整流程（校验+解析+计算）。UI 与 Node 测试都调用它。
// raw: {控件id: 文本}; cs/dj/mc/quyu 单独传。返回 {alert} 或 {bizhi,jielun,jss}。
function runFromRaw(raw, cs, dj, mc, quyu){
  if(!(cs==='1层'||cs==='2层'||cs==='3层')) return {alert:'请选择地下室层数！'};
  if(String(mc||'').trim().length===0 || String(quyu||'').trim().length===0) return {alert:'工程名称、验算区域不能留空，请输入参数！'};

  const groups=[OVERALL, FOUND, F1];
  if(cs==='2层'||cs==='3层') groups.push(F2);
  if(cs==='3层') groups.push(F3);
  const areaX={id:'quyux', key:'qyx', name:'验算区域x向长度'};
  const areaY={id:'quyuy', key:'qyy', name:'验算区域y向长度'};
  const toParse=[areaX, areaY, ...groups.flat()];

  const p={cs, dj, mc};
  const R={ swS:String(raw.shuiwei??'').trim(), jcdS:String(raw.dibandibiaogao??'').trim(),
            dbgS:String(raw.yidingbanbiaogao??'').trim(), yfthS:String(raw.fuyidingbanfutuhou??'').trim(),
            qyxS:String(raw.quyux??'').trim(), qyyS:String(raw.quyuy??'').trim(), quyuS:quyu };
  // 未用到的 -2/-3 层参数在 C# 里保持 0，其回显也需为 "0"，故预置默认。
  const zeroDefaults=[...(cs==='2层'||cs==='3层'?[]:F2), ...(cs==='3层'?[]:F3)];
  zeroDefaults.forEach(fl=>{ p[fl.key]=Math.fround(0); R[fl.key]='0'; });
  R.jcd = String(raw.dibandibiaogao??'').trim(); R.jcbh = String(raw.dibanhoudu??'').trim();

  const errors=[];
  toParse.forEach(fl=>{
    const t=String(raw[fl.id]??'').trim();
    const v=parseFloatCs(t);
    if(v===null){ errors.push('「'+fl.name+'」为空或数值无效'); p[fl.key]=Math.fround(0); R[fl.key]='0'; }
    else { p[fl.key]=v; R[fl.key]=String(parseFloat(t)); }
  });
  if(errors.length) return {alert:'以下参数为空或数值无效，请检查输入：\r\n'+errors.join('\r\n')};
  if(p.qyx<=0 || p.qyy<=0) return {alert:'验算区域x向、y向长度必须大于0！'};

  const r=jisuan(p, R);
  if(!r.ok) return {alert:'请选择抗浮设计等级！'};
  return r;
}

if(typeof module!=='undefined' && module.exports){
  module.exports={ jisuan, runFromRaw, parseFloatCs, OVERALL, FOUND, F1, F2, F3 };
}
